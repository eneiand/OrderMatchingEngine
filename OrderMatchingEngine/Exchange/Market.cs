using System;
using System.Collections.Generic;
using System.Timers;
using OrderMatchingEngine.OrderBook;
using OrderMatchingEngine.OrderBook.OrderProcessing;
using OrderMatchingEngine.OrderBook.Stats;

namespace OrderMatchingEngine.Exchange
{
    public class Market
    {
        private readonly Dictionary<Instrument, OrderBook.OrderBook> m_OrderBooks;
        private readonly List<Timer> m_Timers;
        private bool m_TimersStarted;
        private readonly Object m_Locker = new Object();

        public Market(IDictionary<Instrument, OrderBook.OrderBook> orderBooks, IEnumerable<Timer> timers)
        {
            if (orderBooks == null) throw new ArgumentNullException("orderBooks");

            m_OrderBooks = new Dictionary<Instrument, OrderBook.OrderBook>(orderBooks);
            m_Timers = new List<Timer>(timers);
            m_Timers.ForEach(t => t.Stop());
        }

        public Market(IDictionary<Instrument, OrderBook.OrderBook> orderBooks)
        {
            if (orderBooks == null) throw new ArgumentNullException("orderBooks");

            m_OrderBooks = new Dictionary<Instrument, OrderBook.OrderBook>(orderBooks);
            m_Timers = new List<Timer> {OrdersPerSecondPriority()};
        }

        private Timer OrdersPerSecondPriority()
        {
            var numOrdersPerSecondMonitor = new Timer {AutoReset = true, Enabled = false, Interval = 1000};
            numOrdersPerSecondMonitor.Elapsed += delegate
                                                     {
                                                         var ob = new List<OrderBook.OrderBook>();
                                                         foreach (var orderBook in m_OrderBooks)
                                                         {
                                                             ob.Add(orderBook.Value);
                                                         }

                                                         PrioritiseOrderBooks(ob,
                                                                              (x, y) =>
                                                                              -1*
                                                                              x.Statistics[Statistics.Stat.NumOrders].
                                                                                  Value.CompareTo(
                                                                                      y.Statistics[
                                                                                          Statistics.Stat.NumOrders].
                                                                                          Value));
                                                         ResetStats(ob);
                                                     };
            return numOrdersPerSecondMonitor;
        }

        private static void ResetStats(List<OrderBook.OrderBook> ob)
        {
            ob.ForEach(o => o.Statistics[Statistics.Stat.NumOrders].Reset());
        }

        public static void PrioritiseOrderBooks(List<OrderBook.OrderBook> orderBooks,
                                                Comparison<OrderBook.OrderBook> orderBookComparer, //reads as "sort the OrderBooks by orderBookComparer
                                                int dedicatedThreadsPercentage = 10,    // the top 10% of the sorted OrderBooks should have dedicated threads,
                                                int threadPooledPercentage = 20)        // the next 20% of the OrderBooks use the ThreadPool
                                                                                        // and the remaining 70% will be synchronouss"
        {
            if (orderBooks == null) throw new ArgumentNullException("orderBooks");
            if (orderBookComparer == null) throw new ArgumentNullException("orderBookComparer");

            orderBooks.Sort(orderBookComparer);

            for (int i = 0; i < orderBooks.Count; ++i)
            {
                decimal percentageOfBooks = ((i + 1) / (decimal)orderBooks.Count) * 100m;
                OrderBook.OrderBook oBook = orderBooks[i];
                int limitForThreadPooled = dedicatedThreadsPercentage + threadPooledPercentage;

                if (percentageOfBooks <= dedicatedThreadsPercentage && !(oBook.OrderProcessingStrategy is DedicatedThreadsOrderProcessor))
                    oBook.OrderProcessingStrategy = new DedicatedThreadsOrderProcessor(oBook.BuyOrders, oBook.SellOrders,
                                                                                       oBook.Trades);
                else if (percentageOfBooks <= limitForThreadPooled && !(oBook.OrderProcessingStrategy is ThreadPooledOrderProcessor))
                    oBook.OrderProcessingStrategy = new ThreadPooledOrderProcessor(oBook.BuyOrders, oBook.SellOrders,
                                                                                   oBook.Trades);
                else if (!(oBook.OrderProcessingStrategy is SynchronousOrderProcessor))
                    oBook.OrderProcessingStrategy = new SynchronousOrderProcessor(oBook.BuyOrders, oBook.SellOrders,
                                                                                  oBook.Trades);
            }
        }

        public OrderBook.OrderBook this[Instrument instrument]
        {
            get
            {
                if (instrument == null) throw new ArgumentNullException("instrument");

                OrderBook.OrderBook orderBook;

                if (m_OrderBooks.TryGetValue(instrument, out orderBook))
                    return orderBook;
                else
                    throw new InstrumentNotInThisMarketException();
            }
        }

        public void SubmitOrder(Order order)
        {
            if (order == null) throw new ArgumentNullException("order");

            OrderBook.OrderBook orderBook = this[order.Instrument];
            StartTimersOnFirstOrder();
            orderBook.InsertOrder(order);
        }

        private void StartTimersOnFirstOrder()
        {
            lock (m_Locker)
            {
                if (!m_TimersStarted)
                {
                    m_TimersStarted = true;
                    m_Timers.ForEach(t => t.Start());
                }
            }
        }

        public class MarketName
        {
            public MarketName(String name)
            {
                if (String.IsNullOrEmpty(name.Trim())) throw new ArgumentNullException("name");

                Name = name;
            }

            public String Name { get; private set; }
        }

        public class InstrumentNotInThisMarketException : Exception
        {
            public InstrumentNotInThisMarketException()
                : base("instrument does not have an orderbook in this market")
            {
            }
        }
    }
}
