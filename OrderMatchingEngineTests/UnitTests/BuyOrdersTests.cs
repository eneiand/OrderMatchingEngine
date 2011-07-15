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
    class BuyOrdersTests
    {
        private BuyOrders m_BuyOrders;
        private Instrument m_Instrument;

        [SetUp]
        public void Init()
        {
            m_Instrument = new Instrument("MSFT");
            m_BuyOrders = new BuyOrders(m_Instrument);

            for(int i= 0, j = 10 ; i < 10; ++i, ++j)
            {
                Thread.Sleep(2);
                m_BuyOrders.Insert(new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Buy, 5, (ulong)j));
            }

            for (int i = 0, j = 10; i < 10; ++i, ++j)
            {
                m_BuyOrders.Insert(new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Buy, i, (ulong)j));
            }

        }

        [Test]
        public void CorrectOrderPriorityTest()
        {
            var sortedOrders = new List<Order>(m_BuyOrders);

            for(int i =0; i < sortedOrders.Count-1; ++i)
            {
                var thisOrder = sortedOrders[i];
                var nextOrder = sortedOrders[i + 1];

                Assert.That(thisOrder.Price > nextOrder.Price ||
                            (thisOrder.Price == nextOrder.Price && (thisOrder.CreationTime <= nextOrder.CreationTime)));
            }
        }


    }
}
