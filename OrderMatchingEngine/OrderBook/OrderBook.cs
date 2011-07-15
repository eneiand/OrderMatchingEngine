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

        public Instrument Instrument { get; private set; }
        public BuyOrders BuyOrders { get; private set; }
        public SellOrders SellOrders { get; private set; }
        public Trades Trades { get; private set; }
        public OrderProcessor OrderProcessingStrategy
        {
            get { return m_OrderProcessingStrategy; }
            set
            {
                DedicatedThreadOrderProcessor dedicatedThreadOrderProcessor =
                    m_OrderProcessingStrategy as DedicatedThreadOrderProcessor;

                if (dedicatedThreadOrderProcessor != null)
                    dedicatedThreadOrderProcessor.Stop = true;

                m_OrderProcessingStrategy = value;
            }
        }

        public OrderBook(Instrument instrument, BuyOrders buyOrders, SellOrders sellOrders, Trades trades, OrderProcessor orderProcessingStrategy)
        {
            if (instrument == null) throw new ArgumentNullException("instrument");
            if (buyOrders == null) throw new ArgumentNullException("buyOrders");
            if (sellOrders == null) throw new ArgumentNullException("sellOrders");
            if (trades == null) throw new ArgumentNullException("trades");
            if (orderProcessingStrategy == null) throw new ArgumentNullException("orderProcessingStrategy");
            if( !(instrument == buyOrders.Instrument && instrument == sellOrders.Instrument)) throw new ArgumentException("instrument does not match buyOrders and sellOrders instrument");

            Instrument = instrument;
            BuyOrders = buyOrders;
            SellOrders = sellOrders;
            Trades = trades;
            OrderProcessingStrategy = orderProcessingStrategy;
        }

        public OrderBook(Instrument instrument): this(instrument, new BuyOrders(instrument), new SellOrders(instrument), new Trades())
        {}

        public OrderBook(Instrument instrument, BuyOrders buyOrders, SellOrders sellOrders, Trades trades): this(instrument, buyOrders, sellOrders, trades, new SynchronousOrderProcessor(buyOrders, sellOrders, trades))
        {}

        public void InsertOrder(Order order)
        {
            this.OrderProcessingStrategy.InsertOrder(order);
        }



        public abstract class OrderProcessor
        {
            protected BuyOrders m_BuyOrders;
            protected SellOrders m_SellOrders;
            protected Trades m_Trades;

            public OrderProcessor(BuyOrders buyOrders, SellOrders sellOrders, Trades trades)
            {
                m_BuyOrders = buyOrders;
                m_SellOrders = sellOrders;
                m_Trades = trades;
            }

            public abstract void InsertOrder(Order order);

            protected void ProcessOrder(Order order)
            {
                switch (order.BuySell)
                {
                    case Order.BuyOrSell.Buy:
                        m_BuyOrders.Insert(order);
                        break;
                    case Order.BuyOrSell.Sell:
                        m_SellOrders.Insert(order);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public class SynchronousOrderProcessor : OrderProcessor
        {
            public SynchronousOrderProcessor(BuyOrders buyOrders, SellOrders sellOrders, Trades trades) : base(buyOrders, sellOrders, trades) { }

            public override void InsertOrder(Order order)
            {
                ProcessOrder(order);
            }
        }

        public class ThreadPooledOrderProcessor : OrderProcessor
        {
            public ThreadPooledOrderProcessor(BuyOrders buyOrders, SellOrders sellOrders, Trades trades) : base(buyOrders, sellOrders, trades) { }

            public override void InsertOrder(Order order)
            {
                ThreadPool.QueueUserWorkItem((o) => ProcessOrder(order));
            }
        }

        public class DedicatedThreadOrderProcessor : OrderProcessor
        {
            private Thread m_Thread;
            private AutoResetEvent m_OrderReceivedEvent = new AutoResetEvent(false);
            private ConcurrentQueue<Order> m_PendingOrders = new ConcurrentQueue<Order>();

            public DedicatedThreadOrderProcessor(BuyOrders buyOrders, SellOrders sellOrders, Trades trades) : base(buyOrders, sellOrders, trades)
            {
                m_Thread = new Thread(new ThreadStart(StartProcessingOrders));
                m_Thread.Start();
            }

            private void StartProcessingOrders()
            {
                while(!Stop)
                {
                    m_OrderReceivedEvent.WaitOne();
                    ProcessOrders();
                }

                //make sure to finish any pending orders 
                ProcessOrders();
            }

            private void ProcessOrders()
            {
                while (m_PendingOrders.Count > 0)
                {
                    Order order;
                    m_PendingOrders.TryDequeue(out order);

                    ProcessOrder(order);
                }
            }

            public bool Stop { get; set; }

            public override void InsertOrder(Order order)
            {
                m_PendingOrders.Enqueue(order);
                m_OrderReceivedEvent.Set();
            }

        }
    }
}
