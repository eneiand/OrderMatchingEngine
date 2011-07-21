using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OrderMatchingEngine.OrderBook;
using OrderMatchingEngine.OrderBook.OrderProcessing;

namespace OrderMatchingEngineTests.UnitTests
{
    [TestFixture]
    class OrderProcessorTests
    {
        private Instrument m_Instrument;
        private OrderBook m_OrderBook;

        [SetUp]
        public void Init()
        {
            m_Instrument = new Instrument("GOOG");
            m_OrderBook = new OrderBook(m_Instrument);
        }

        [Test]
        public void SwitchingOrderProcessingStrategiesTest()
        {
            List<Order> orders = new List<Order>(Orders());

            for (int i = 0; i < 40; ++i)
                m_OrderBook.InsertOrder(orders[i]);

            m_OrderBook.OrderProcessingStrategy = new ThreadPooledOrderProcessor(m_OrderBook.BuyOrders, m_OrderBook.SellOrders, m_OrderBook.Trades);

            for (int i = 40; i < 80; ++i)
                m_OrderBook.InsertOrder(orders[i]);

            m_OrderBook.OrderProcessingStrategy = new DedicatedThreadsOrderProcessor(m_OrderBook.BuyOrders, m_OrderBook.SellOrders, m_OrderBook.Trades);

            for (int i = 80; i < 120; ++i)
                m_OrderBook.InsertOrder(orders[i]);

            m_OrderBook.OrderProcessingStrategy = new ThreadPooledOrderProcessor(m_OrderBook.BuyOrders, m_OrderBook.SellOrders, m_OrderBook.Trades);

            for (int i = 120; i < 160; ++i)
                m_OrderBook.InsertOrder(orders[i]);

            m_OrderBook.OrderProcessingStrategy = new SynchronousOrderProcessor(m_OrderBook.BuyOrders, m_OrderBook.SellOrders, m_OrderBook.Trades);

            for (int i = 160; i < orders.Count; ++i)
                m_OrderBook.InsertOrder(orders[i]);

            while (m_OrderBook.SellOrders.Count() != 100 || m_OrderBook.BuyOrders.Count() != 100) ;

            Assert.That(m_OrderBook.BuyOrders.Count(), Is.EqualTo(100));
            Assert.That(m_OrderBook.SellOrders.Count(), Is.EqualTo(100));
         }


        private IEnumerable<Order> Orders(int numBuyOrders = 100, int numSellOrders = 100)
        {
            for (int i = 1; i <= numBuyOrders; ++i)
               yield return new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Buy, 100m, 100ul);

            for (int i = 1; i <= numSellOrders; ++i)
                yield return new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Sell, 110m, 110);

        }
    }
}
