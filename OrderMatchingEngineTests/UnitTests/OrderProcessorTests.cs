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
        private Trades m_Trades;
        private Trades.InMemoryTradeProcessor m_TradeProcessor;

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
            m_Trades = new Trades(m_Instrument);
            m_TradeProcessor = m_Trades.TradeProcessingStrategy as Trades.InMemoryTradeProcessor;
        }

        [Test]
        public void MatchBuyOrderTest()
        {

            var buyQuantity = m_BuyOrder.Quantity;

            Assert.True(OrderBook.OrderProcessor.TryMatchOrder(m_BuyOrder, m_SellOrders, m_Trades));
            var trade = m_TradeProcessor.Trades[0];
           
            Assert.That(trade.Instrument, Is.EqualTo(m_Instrument));
            Assert.That(trade.Price, Is.EqualTo(m_SellOrder.Price));
            Assert.That(trade.Quantity, Is.EqualTo(buyQuantity));

            Assert.That(m_SellOrders.Count() == 0);
        }

        [Test]
        public void MatchSellOrderTest()
        {

            var sellQuantity = m_SellOrder.Quantity;

            Assert.True(OrderBook.OrderProcessor.TryMatchOrder(m_SellOrder, m_BuyOrders, m_Trades));
            var trade = m_TradeProcessor.Trades[0];

            Assert.That(trade.Instrument, Is.EqualTo(m_Instrument));
            Assert.That(trade.Price, Is.EqualTo(m_BuyOrder.Price));
            Assert.That(trade.Quantity, Is.EqualTo(sellQuantity));

            Assert.That(m_BuyOrders.Count() == 0);
        }

        [Test]
        public void NoMatchTest()
        {
            Assert.False(OrderBook.OrderProcessor.TryMatchOrder(new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Sell, m_BuyOrder.Price + 10, 100ul),
                m_BuyOrders, m_Trades));
            Assert.That(this.m_TradeProcessor.Trades.Count, Is.EqualTo(0));
            Assert.That(m_BuyOrders.Count() == 1);
        }

        [Test]
        public void BigBuyMatchesMultipleSellsTest()
        {
            var secondSellOrder = new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled,
                                                  Order.BuyOrSell.Sell, 90, 100);
            m_SellOrders.Insert(secondSellOrder);
            m_BuyOrder.Quantity = 150;
            Assert.True(OrderBook.OrderProcessor.TryMatchOrder(m_BuyOrder, m_SellOrders, m_Trades));

            Assert.That(this.m_TradeProcessor.Trades.Count, Is.EqualTo(2));
            Assert.That(m_BuyOrder.Quantity == 0);
            Assert.That(secondSellOrder.Quantity == 50);
            Assert.That(m_SellOrders[0] == secondSellOrder && m_SellOrders.Count() == 1);
        }

        [Test]
        public void BigSellMatchesMultipleBuysTest()
        {
            var secondBuyOrder = new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled,
                                                  Order.BuyOrSell.Buy, 90, 100);
            m_BuyOrders.Insert(secondBuyOrder);
            m_SellOrder.Quantity = 150;
            Assert.True(OrderBook.OrderProcessor.TryMatchOrder(m_SellOrder, m_BuyOrders, m_Trades));

            Assert.That(this.m_TradeProcessor.Trades.Count, Is.EqualTo(2));
            Assert.That(m_BuyOrder.Quantity == 0);
            Assert.That(secondBuyOrder.Quantity == 50);
            Assert.That(m_BuyOrders[0] == secondBuyOrder && m_BuyOrders.Count() == 1);
        }

        [Test]
        public void LittleSellMatchesPartialBuyTest()
        {
            m_SellOrder.Quantity = 50;
            Assert.True(OrderBook.OrderProcessor.TryMatchOrder(m_SellOrder, m_BuyOrders, m_Trades));

            Assert.That(this.m_TradeProcessor.Trades.Count, Is.EqualTo(1));
            Assert.That(m_BuyOrder.Quantity == 50);

            Assert.That(m_BuyOrders[0] == m_BuyOrder && m_BuyOrders.Count() == 1);
            Assert.That(m_SellOrder.Quantity == 0);

        }

        [Test]
        public void LittleBuyMatchesPartialSellTest()
        {
            m_BuyOrder.Quantity = 50;
            Assert.True(OrderBook.OrderProcessor.TryMatchOrder(m_BuyOrder, m_SellOrders, m_Trades));

            Assert.That(this.m_TradeProcessor.Trades.Count, Is.EqualTo(1));
            Assert.That(m_SellOrder.Quantity == 50);

            Assert.That(m_SellOrders[0] == m_SellOrder && m_SellOrders.Count() == 1);
            Assert.That(m_BuyOrder.Quantity == 0);
        }
    }
}
