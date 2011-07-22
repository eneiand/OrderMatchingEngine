using System;
using System.Collections.Generic;
using OrderMatchingEngine.Exchange;
using OrderMatchingEngine.OrderBook.OrderProcessing;
using OrderMatchingEngine.OrderBook.Stats;

namespace OrderMatchingEngine.OrderBook
{
    public class OrderBook
    {
        private OrderProcessor m_OrderProcessingStrategy;

        private readonly Object m_Locker = new Object();

        public Instrument Instrument { get; private set; }
        public BuyOrders BuyOrders { get; private set; }
        public SellOrders SellOrders { get; private set; }
        public Trades Trades { get; private set; }
        public Statistics Statistics { get; private set; }

        public OrderProcessor OrderProcessingStrategy
        {
            get { return m_OrderProcessingStrategy; }
            set
            {
                lock (m_Locker)
                {
                    var dedicatedThreadOrderProcessor = m_OrderProcessingStrategy as DedicatedThreadsOrderProcessor;

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
            Statistics = new Statistics();
        }

        public OrderBook(Instrument instrument)
            : this(instrument, new BuyOrders(instrument), new SellOrders(instrument), new Trades(instrument))
        {
        }

        public OrderBook(Instrument instrument, BuyOrders buyOrders, SellOrders sellOrders, Trades trades)
            : this(
                instrument, buyOrders, sellOrders, trades, new SynchronousOrderProcessor(buyOrders, sellOrders, trades))
        {
        }

        public void InsertOrder(Order order)
        {
            if (order == null) throw new ArgumentNullException("order");
            if (order.Instrument != Instrument)
                throw new OrderIsNotForThisBookException();

            OrderReceived();

            //the strategy can change at runtime so lock here and in OrderProcessingStrategy property
            lock (m_Locker)
                OrderProcessingStrategy.InsertOrder(order);
        }

        private void OrderReceived()
        {
            var numOrders = Statistics[Statistics.Stat.NumOrders];
            numOrders++;
        }

        public class OrderIsNotForThisBookException : Exception
        {
        }
    }
}