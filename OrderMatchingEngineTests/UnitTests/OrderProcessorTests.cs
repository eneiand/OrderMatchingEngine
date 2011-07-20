using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OrderMatchingEngine;
using OrderMatchingEngine.OrderBook;

namespace OrderMatchingEngineTests.UnitTests
{
    [TestFixture]
    class OrderProcessorTests
    {
        private EquityOrder m_BuyOrder, m_SellOrder;
        private Instrument m_Instrument;
        private SellOrders m_SellOrders;
        private BuyOrders m_BuyOrders;

        [SetUp]
        public void Init()
        {
            m_Instrument = new Instrument("GOOG");
            m_BuyOrder = new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Buy, 100M, 100ul);
            m_SellOrder = new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Sell, 90, 100ul);
            m_SellOrders = new SellOrders(m_Instrument);
            m_BuyOrders = new BuyOrders(m_Instrument);
            m_SellOrders.Insert(m_SellOrder);
            m_BuyOrders.Insert(m_BuyOrder);
        }

        [Test]
        public void MatchBuyOrderTest()
        {
            Trade trade;

            var buyQuantity = m_BuyOrder.Quantity;

            OrderBook.OrderProcessor.TryMatchOrder(m_BuyOrder, m_SellOrders, out trade);

            Assert.That(trade.Instrument, Is.EqualTo(m_Instrument));
            Assert.That(trade.Price, Is.EqualTo(m_SellOrder.Price));
            Assert.That(trade.Quantity, Is.EqualTo(buyQuantity));

            Assert.That(m_SellOrders.Count() == 0);
        }

        [Test]
        public void MatchSellOrderTest()
        {
            Trade trade;

            var sellQuantity = m_SellOrder.Quantity;

            OrderBook.OrderProcessor.TryMatchOrder(m_SellOrder, m_BuyOrders, out trade);

            Assert.That(trade.Instrument, Is.EqualTo(m_Instrument));
            Assert.That(trade.Price, Is.EqualTo(m_BuyOrder.Price));
            Assert.That(trade.Quantity, Is.EqualTo(sellQuantity));

            Assert.That(m_BuyOrders.Count() == 0);
        }
    }
}
