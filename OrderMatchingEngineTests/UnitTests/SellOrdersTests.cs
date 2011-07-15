using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using OrderMatchingEngine;
using OrderMatchingEngine.OrderBook;

namespace OrderMatchingEngineTests.UnitTests
{
    [TestFixture]
    class SellOrdersTests
    {
        private SellOrders m_SellOrders;
        private Instrument m_Instrument;

        [SetUp]
        public void Init()
        {
            m_Instrument = new Instrument("MSFT");
            m_SellOrders = new SellOrders(m_Instrument);

            for (int i = 0, j = 10; i < 10; ++i, ++j)
            {
                Thread.Sleep(2);
                m_SellOrders.Insert(new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Sell, 5, (ulong)j));
            }

            for (int i = 0, j = 10; i < 10; ++i, ++j)
            {
                m_SellOrders.Insert(new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Sell, i, (ulong)j));
            }

        }

        [Test]
        public void CorrectOrderPriorityTest()
        {
            var sortedOrders = new List<Order>(m_SellOrders);

            for (int i = 0; i < sortedOrders.Count - 1; ++i)
            {
                var thisOrder = sortedOrders[i];
                var nextOrder = sortedOrders[i + 1];

                Assert.That(thisOrder.Price < nextOrder.Price ||
                            (thisOrder.Price == nextOrder.Price && (thisOrder.CreationTime <= nextOrder.CreationTime)));
            }
        }

        [Test]
        public void WrongOrderTypeThrowsException()
        {
            var order = new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Buy, 0, 0);

            Assert.Throws<ArgumentException>(() => m_SellOrders.Insert(order));
        }
        [Test]
        public void WrongInstrumentThrowsException()
        {
            var order = new EquityOrder(new Instrument("WRONG"), Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Sell, 0, 0);

            Assert.Throws<ArgumentException>(() => m_SellOrders.Insert(order));
        }
    }
}
