using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OrderMatchingEngine.OrderBook
{
    public class OrderBook
    {
        private OrderProcessor m_OrderProcessingStrategy;
        private Object m_Locker = new Object();

        public Instrument Instrument { get; private set; }
        public BuyOrders BuyOrders { get; private set; }
        public SellOrders SellOrders { get; private set; }
        public Trades Trades { get; private set; }

        public OrderProcessor OrderProcessingStrategy
        {
            get { return m_OrderProcessingStrategy; }
            set
            {
                
                lock(m_Locker)
                {
                    DedicatedThreadOrderProcessor dedicatedThreadOrderProcessor = m_OrderProcessingStrategy as DedicatedThreadOrderProcessor;

                    if (dedicatedThreadOrderProcessor != null)
                        dedicatedThreadOrderProcessor.Stop();

                    m_OrderProcessingStrategy = value;
                }
            }
        }

        public OrderBook(Instrument instrument, BuyOrders buyOrders, SellOrders sellOrders, Trades trades,
                         OrderProcessor orderProcessingStrategy)
        {
            if (instrument == null) throw new ArgumentNullException("instrument");
            if (buyOrders == null) throw new ArgumentNullException("buyOrders");
            if (sellOrders == null) throw new ArgumentNullException("sellOrders");
            if (trades == null) throw new ArgumentNullException("trades");
            if (orderProcessingStrategy == null) throw new ArgumentNullException("orderProcessingStrategy");
            if (!(instrument == buyOrders.Instrument && instrument == sellOrders.Instrument))
                throw new ArgumentException("instrument does not match buyOrders and sellOrders instrument");

            Instrument = instrument;
            BuyOrders = buyOrders;
            SellOrders = sellOrders;
            Trades = trades;
            OrderProcessingStrategy = orderProcessingStrategy;
        }

        public OrderBook(Instrument instrument)
            : this(instrument, new BuyOrders(instrument), new SellOrders(instrument), new Trades())
        {
        }

        public OrderBook(Instrument instrument, BuyOrders buyOrders, SellOrders sellOrders, Trades trades)
            : this(
                instrument, buyOrders, sellOrders, trades, new SynchronousOrderProcessor(buyOrders, sellOrders, trades))
        {
        }

        public void InsertOrder(Order order)
        {
            //the strategy can change at runtime so lock here and in OrderProcessingStrategy property
            lock(m_Locker)
                this.OrderProcessingStrategy.InsertOrder(order);
        }



        public abstract class OrderProcessor
        {
            protected static bool TryMatchOrder(Order order, Orders orders)
            {
                IEnumerable<Order> candidateOrders = order.BuySell == Order.BuyOrSell.Buy
                                                         ? orders.FindAll(o => o.Price <= order.Price)
                                                         : orders.FindAll(o => o.Price >= order.Price);
                    
                    //foreach (var candidateOrder in candidateOrders)
                    //{
                    //    if (order.Quantity > 0)
                    //    {
                    //        var quantity = candidateOrder.Quantity;

                    //        candidateOrder.Quantity -= order.Quantity;
                    //        order.Quantity -= quantity;
                    //    }
                    //    else
                    //    {
                    //        return true;
                    //    }
                    //}
                    //return true;
                return false;

            }

            protected BuyOrders m_BuyOrders;
            protected SellOrders m_SellOrders;
            protected Trades m_Trades;

            public Func<Order, Orders, bool> TryMatchBuyOrder { get; set; }
            public Func<Order, Orders, bool> TryMatchSellOrder { get; set; }



            public OrderProcessor(BuyOrders buyOrders, SellOrders sellOrders, Trades trades, Func<Order, Orders, bool> tryMatchBuyOrder, Func<Order, Orders, bool> tryMatchSellOrder)
            {
                m_BuyOrders = buyOrders;
                m_SellOrders = sellOrders;
                m_Trades = trades;
                TryMatchBuyOrder = tryMatchBuyOrder;
                TryMatchSellOrder = tryMatchSellOrder;
            }

            public OrderProcessor(BuyOrders buyOrders, SellOrders sellOrders, Trades trades)
                : this(buyOrders, sellOrders, trades,
                    TryMatchOrder,
                    TryMatchOrder)
            {

            }


            public abstract void InsertOrder(Order order);

            protected void ProcessOrder(Order order)
            {
                switch (order.BuySell)
                {
                    case Order.BuyOrSell.Buy:
                        if (!TryMatchBuyOrder(order, this.m_SellOrders))
                            m_BuyOrders.Insert(order);
                        break;
                    case Order.BuyOrSell.Sell:
                        if (!TryMatchSellOrder(order, this.m_BuyOrders))
                            m_SellOrders.Insert(order);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

        }

        public class SynchronousOrderProcessor : OrderBook.OrderProcessor
        {
            public SynchronousOrderProcessor(BuyOrders buyOrders, SellOrders sellOrders, Trades trades)
                : base(buyOrders, sellOrders, trades)
            {
            }

            public override void InsertOrder(Order order)
            {
                ProcessOrder(order);
            }
        }

        public class ThreadPooledOrderProcessor : OrderBook.OrderProcessor
        {
            public ThreadPooledOrderProcessor(BuyOrders buyOrders, SellOrders sellOrders, Trades trades)
                : base(buyOrders, sellOrders, trades)
            {
            }

            public override void InsertOrder(Order order)
            {
                ThreadPool.QueueUserWorkItem((o) => ProcessOrder(order));
            }
        }

        public class DedicatedThreadOrderProcessor : OrderBook.OrderProcessor
        {
            private readonly Thread m_Thread;
            private readonly BlockingCollection<Order> m_PendingOrders = new BlockingCollection<Order>();

            public DedicatedThreadOrderProcessor(BuyOrders buyOrders, SellOrders sellOrders, Trades trades)
                : base(buyOrders, sellOrders, trades)
            {
                m_Thread = new Thread(ProcessOrders);
                m_Thread.Start();
            }


            private void ProcessOrders()
            {
                foreach (var order in m_PendingOrders.GetConsumingEnumerable())
                {
                    ProcessOrder(order);
                }
            }

            public void Stop()
            {
                this.m_PendingOrders.CompleteAdding();
            }

            public override void InsertOrder(Order order)
            {
                m_PendingOrders.Add(order);
            }

        }
    }

}