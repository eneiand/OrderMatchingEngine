﻿using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using OrderMatchingEngine.OrderBook;

namespace OrderMatchingEngineTests.UnitTests
{
    [TestFixture]
    internal class BuyOrdersTests
    {
        private BuyOrders m_BuyOrders;
        private Instrument m_Instrument;

        [SetUp]
        public void Init()
        {
            m_Instrument = new Instrument("MSFT");
            m_BuyOrders = new BuyOrders(m_Instrument);

            for (int i = 0, j = 10; i < 10; ++i, ++j)
            {
                Thread.Sleep(2);
                m_BuyOrders.Insert(new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled,
                                                   Order.BuyOrSell.Buy, 5, (ulong)j));
            }

            for (int i = 0, j = 10; i < 10; ++i, ++j)
            {
                m_BuyOrders.Insert(new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled,
                                                   Order.BuyOrSell.Buy, 5, (ulong)j));
            }
        }

        [Test]
        public void CorrectOrderPriorityTest()
        {
            var sortedOrders = new List<Order>(m_BuyOrders);

            for (int i = 0; i < sortedOrders.Count - 1; ++i)
            {
                Order thisOrder = sortedOrders[i];
                Order nextOrder = sortedOrders[i + 1];

                Assert.That(thisOrder.Price > nextOrder.Price ||
                            (thisOrder.Price == nextOrder.Price && (thisOrder.CreationTime <= nextOrder.CreationTime)));
            }
        }

        [Test]
        public void WrongOrderTypeThrowsException()
        {
            var order = new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Sell, 5, 10ul);

            Assert.Throws<ArgumentException>(() => m_BuyOrders.Insert(order));
        }

        [Test]
        public void WrongInstrumentThrowsException()
        {
            var order = new EquityOrder(new Instrument("WRONG"), Order.OrderTypes.GoodUntilCancelled,
                                        Order.BuyOrSell.Buy, 5, 10ul);

            Assert.Throws<ArgumentException>(() => m_BuyOrders.Insert(order));
        }
    }
}
