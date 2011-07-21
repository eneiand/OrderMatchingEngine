using System.Collections.Generic;
using NUnit.Framework;
using OrderMatchingEngine.OrderBook;
using OrderMatchingEngine.OrderBook.OrderProcessing;

namespace OrderMatchingEngineTests.UnitTests
{
    [TestFixture]
    internal class OrderBookTests
    {
        private OrderBook m_OrderBook;
        private Instrument m_Instrument;
        private BuyOrders m_BuyOrders;
        private SellOrders m_SellOrders;
        private Trades m_Trades;
        private List<Order> m_Orders;

        [SetUp]
        public void Init()
        {
            m_Instrument = new Instrument("MSFT");
            m_BuyOrders = new BuyOrders(m_Instrument);
            m_SellOrders = new SellOrders(m_Instrument);
            m_Trades = new Trades(m_Instrument);

            m_OrderBook = new OrderBook(m_Instrument, m_BuyOrders, m_SellOrders, m_Trades);

            m_Orders = new List<Order>
            {
                new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Buy, 100, 100),
                new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilDate, Order.BuyOrSell.Sell, 110, 100)
            };
        }

        [Test]
        public void InsertOrderTest()
        {
            foreach (Order order in m_Orders)
                m_OrderBook.InsertOrder(order);

            Assert.That(m_OrderBook.BuyOrders[0], Is.EqualTo(m_Orders[0]));
            Assert.That(m_OrderBook.SellOrders[0], Is.EqualTo(m_Orders[1]));
        }

        [Test, Timeout(500)]
        public void InsertOrderPoolTest()
        {
            m_OrderBook.OrderProcessingStrategy = new ThreadPooledOrderProcessor(m_BuyOrders, m_SellOrders,
                                                                                           m_Trades);

            foreach (Order order in m_Orders)
                m_OrderBook.InsertOrder(order);

            List<Order> buys;
            List<Order> sells;

            do
            {
                buys = new List<Order>(m_BuyOrders);
                sells = new List<Order>(m_SellOrders);
            }
            while (buys.Count == 0 || sells.Count == 0); //spin wait for thread pool

            Assert.That(m_OrderBook.BuyOrders[0], Is.EqualTo(m_Orders[0]));
            Assert.That(m_OrderBook.SellOrders[0], Is.EqualTo(m_Orders[1]));
        }

        [Test, Timeout(500)]
        public void InsertOrderDedicatedThreadTest()
        {
            m_OrderBook.OrderProcessingStrategy = new DedicatedThreadsOrderProcessor(m_BuyOrders, m_SellOrders,
                                                                                              m_Trades);

            foreach (Order order in m_Orders)
                m_OrderBook.InsertOrder(order);

            List<Order> buys;
            List<Order> sells;

            do
            {
                buys = new List<Order>(m_BuyOrders);
                sells = new List<Order>(m_SellOrders);
            }
            while (buys.Count == 0 || sells.Count == 0); //spin wait for thread pool

            Assert.That(m_OrderBook.BuyOrders[0], Is.EqualTo(m_Orders[0]));
            Assert.That(m_OrderBook.SellOrders[0], Is.EqualTo(m_Orders[1]));
        }
    }
}
