using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OrderMatchingEngine.Exchange;
using OrderMatchingEngine.OrderBook;
using OrderMatchingEngine.OrderBook.Stats;

namespace OrderMatchingEngineTests.UnitTests
{
    [TestFixture]
    internal class MarketTests
    {
        private Market m_Market;
        private Instrument m_Instrument;
        private OrderBook m_OrderBook;

        [SetUp]
        public void Setup()
        {
            m_Instrument = new Instrument("MSFT");
            m_OrderBook = new OrderBook(m_Instrument);
            var orderBooks = new Dictionary<Instrument, OrderBook>();
            orderBooks[m_Instrument] = m_OrderBook;
            m_Market = new Market(orderBooks);
        }

        [Test]
        public void SubmitBuyOrderTest()
        {
            var buyOrder = new EquityOrder(m_Instrument, Order.OrderTypes.LimitPrice, Order.BuyOrSell.Buy, 100, 100);
            m_Market.SubmitOrder(buyOrder);

            Assert.That(m_Market[m_Instrument].BuyOrders[0], Is.EqualTo(buyOrder));
        }

        [Test]
        public void SubmitSellOrderTest()
        {
            var sellOrder = new EquityOrder(m_Instrument, Order.OrderTypes.LimitPrice, Order.BuyOrSell.Sell, 100, 100);
            m_Market.SubmitOrder(sellOrder);

            Assert.That(m_Market[m_Instrument].SellOrders[0], Is.EqualTo(sellOrder));
        }

        [Test]
        public void TrySubmitAnOrderForTheWrongInstrument()
        {
            Assert.Throws<Market.InstrumentNotInThisMarketException>(
                () =>
                m_Market.SubmitOrder(new EquityOrder(new Instrument("XXXX"), Order.OrderTypes.GoodUntilCancelled,
                                                     Order.BuyOrSell.Buy, 100, 100)));
        }

        [Test]
        public void SubmitMultipleOrdersConcurrently()
        {
            var orders = new List<Order>(Orders());


            foreach (Order order in orders)
            {
                Order order1 = order;
                ThreadPool.QueueUserWorkItem((e) => m_Market.SubmitOrder(order1));
            }

            while (m_OrderBook.SellOrders.Count() != 2 || m_OrderBook.BuyOrders.Count() != 2) ;

            Assert.That(orders[0], Is.EqualTo(m_OrderBook.BuyOrders[1]));
            Assert.That(orders[1], Is.EqualTo(m_OrderBook.BuyOrders[0]));
            Assert.That(orders[2], Is.EqualTo(m_OrderBook.SellOrders[0]));
            Assert.That(orders[3], Is.EqualTo(m_OrderBook.SellOrders[1]));
        }


        [Test]
        public void OrderBooksPrioritiserTest()
        {
            var orderBooks = new List<OrderBook>();

            for (int i = 0; i < 10; ++i)
            {
                var book = new OrderBook(new Instrument("" + i));
                for (int j = 0; j < i; ++j)
                {
                    Statistic stat = book.Statistics[Statistics.Stat.NumOrders];
                    ++stat;
                }
                orderBooks.Add(book);
            }

            Market.PrioritiseOrderBooks(orderBooks,
                                        (x, y) => -1*x.Statistics[Statistics.Stat.NumOrders].Value.CompareTo(
                                            y.Statistics[Statistics.Stat.NumOrders].Value));

            for(int i = 1; i < orderBooks.Count; ++i)
            {
                Assert.That(orderBooks[i].Statistics[Statistics.Stat.NumOrders].Value, Is.LessThanOrEqualTo(orderBooks[i-1].Statistics[Statistics.Stat.NumOrders].Value));
            }
        }

        private IEnumerable<Order> Orders()
        {
            for (int i = 1; i < 3; ++i)
                yield return
                    new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Buy, i, 10ul);

            for (int i = 3; i < 5; ++i)
                yield return
                    new EquityOrder(m_Instrument, Order.OrderTypes.GoodUntilCancelled, Order.BuyOrSell.Sell, i, 10ul);
        }
    }
}
