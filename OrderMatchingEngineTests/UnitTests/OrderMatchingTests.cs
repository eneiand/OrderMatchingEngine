using System.Linq;
using NUnit.Framework;
using OrderMatchingEngine.OrderBook;
using OrderMatchingEngine.OrderBook.OrderProcessing;

namespace OrderMatchingEngineTests.UnitTests
{
    [TestFixture]
    internal class OrderMatchingTests
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
            m_BuyOrder = new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Buy, 100M,
                                         100ul);
            m_SellOrder = new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Sell, 90,
                                          100ul);
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
            ulong buyQuantity = m_BuyOrder.Quantity;

            Assert.True(OrderProcessor.TryMatchOrder(m_BuyOrder, m_SellOrders, m_Trades));
            Trade trade = m_TradeProcessor.Trades[0];

            Assert.That(trade.Instrument, Is.EqualTo(m_Instrument));
            Assert.That(trade.Price, Is.EqualTo(m_SellOrder.Price));
            Assert.That(trade.Quantity, Is.EqualTo(buyQuantity));

            Assert.That(m_SellOrders.Count() == 0);
        }

        [Test]
        public void OneBuyLotsOfPotentialSellsOrderTest()
        {
            ulong buyQuantity = m_BuyOrder.Quantity;

            m_SellOrders.Insert(new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Sell, 95, 100));
            m_SellOrders.Insert(new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Sell, 95, 100));

            Assert.True(OrderProcessor.TryMatchOrder(m_BuyOrder, m_SellOrders, m_Trades));
            Trade trade = m_TradeProcessor.Trades[0];

            Assert.That(m_TradeProcessor.Trades.Count, Is.EqualTo(1));

            Assert.That(trade.Instrument, Is.EqualTo(m_Instrument));
            Assert.That(trade.Price, Is.EqualTo(m_SellOrder.Price));
            Assert.That(trade.Quantity, Is.EqualTo(buyQuantity));

            Assert.That(m_SellOrders.Count() == 2);
            Assert.That(!m_SellOrders.Contains(m_SellOrder));
        }

        [Test]
        public void MatchSellOrderTest()
        {
            ulong sellQuantity = m_SellOrder.Quantity;

            Assert.True(OrderProcessor.TryMatchOrder(m_SellOrder, m_BuyOrders, m_Trades));
            Trade trade = m_TradeProcessor.Trades[0];

            Assert.That(trade.Instrument, Is.EqualTo(m_Instrument));
            Assert.That(trade.Price, Is.EqualTo(m_BuyOrder.Price));
            Assert.That(trade.Quantity, Is.EqualTo(sellQuantity));

            Assert.That(m_BuyOrders.Count() == 0);
        }

        [Test]
        public void NoMatchTest()
        {
            Assert.False(
                OrderProcessor.TryMatchOrder(
                    new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Sell,
                                    m_BuyOrder.Price + 10, 100ul),
                    m_BuyOrders, m_Trades));
            Assert.That(m_TradeProcessor.Trades.Count, Is.EqualTo(0));
            Assert.That(m_BuyOrders.Count() == 1);
        }

        [Test]
        public void BigBuyMatchesMultipleSellsTest()
        {
            var secondSellOrder = new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled,
                                                  Order.BuyOrSell.Sell, 95, 100);
            m_SellOrders.Insert(secondSellOrder);
            m_BuyOrder.Quantity = 150;
            Assert.True(OrderProcessor.TryMatchOrder(m_BuyOrder, m_SellOrders, m_Trades));

            Assert.That(m_TradeProcessor.Trades.Count, Is.EqualTo(2));
            Assert.That(m_BuyOrder.Quantity, Is.EqualTo(0));
            Assert.That(secondSellOrder.Quantity, Is.EqualTo(50));
            Assert.That(m_SellOrders[0] == secondSellOrder && m_SellOrders.Count() == 1);
        }

        [Test]
        public void BigSellMatchesMultipleBuysTest()
        {
            var secondBuyOrder = new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled,
                                                 Order.BuyOrSell.Buy, 90, 100);
            m_BuyOrders.Insert(secondBuyOrder);
            m_SellOrder.Quantity = 150;
            Assert.True(OrderProcessor.TryMatchOrder(m_SellOrder, m_BuyOrders, m_Trades));

            Assert.That(m_TradeProcessor.Trades.Count, Is.EqualTo(2));
            Assert.That(m_BuyOrder.Quantity == 0);
            Assert.That(secondBuyOrder.Quantity == 50);
            Assert.That(m_BuyOrders[0] == secondBuyOrder && m_BuyOrders.Count() == 1);
        }

        [Test]
        public void LittleSellMatchesPartialBuyTest()
        {
            m_SellOrder.Quantity = 50;
            Assert.True(OrderProcessor.TryMatchOrder(m_SellOrder, m_BuyOrders, m_Trades));

            Assert.That(m_TradeProcessor.Trades.Count, Is.EqualTo(1));
            Assert.That(m_BuyOrder.Quantity == 50);

            Assert.That(m_BuyOrders[0] == m_BuyOrder && m_BuyOrders.Count() == 1);
            Assert.That(m_SellOrder.Quantity == 0);
        }

        [Test]
        public void LittleBuyMatchesPartialSellTest()
        {
            m_BuyOrder.Quantity = 50;
            Assert.True(OrderProcessor.TryMatchOrder(m_BuyOrder, m_SellOrders, m_Trades));

            Assert.That(m_TradeProcessor.Trades.Count, Is.EqualTo(1));
            Assert.That(m_SellOrder.Quantity == 50);

            Assert.That(m_SellOrders[0] == m_SellOrder && m_SellOrders.Count() == 1);
            Assert.That(m_BuyOrder.Quantity == 0);
        }

        [Test]
        public void BigBuyMatchesPartialSellTest()
        {
            m_BuyOrder.Quantity = 150;
            Assert.True(OrderProcessor.TryMatchOrder(m_BuyOrder, m_SellOrders, m_Trades));

            Assert.That(m_TradeProcessor.Trades.Count, Is.EqualTo(1));
            Assert.That(m_SellOrder.Quantity == 0);

            Assert.That(m_BuyOrders[0] == m_BuyOrder && m_BuyOrders.Count() == 1);
            Assert.That(m_SellOrders.Count() == 0);

            Assert.That(m_BuyOrder.Quantity == 50);
        }

        [Test]
        public void BigSellMatchesPartialBuyTest()
        {
            m_SellOrder.Quantity = 150;
            Assert.True(OrderProcessor.TryMatchOrder(m_SellOrder, m_BuyOrders, m_Trades));

            Assert.That(m_TradeProcessor.Trades.Count, Is.EqualTo(1));
            Assert.That(m_BuyOrder.Quantity == 0);

            Assert.That(m_SellOrders[0] == m_SellOrder && m_SellOrders.Count() == 1);
            Assert.That(m_BuyOrders.Count() == 0);

            Assert.That(m_SellOrder.Quantity == 50);
        }
    }
}
