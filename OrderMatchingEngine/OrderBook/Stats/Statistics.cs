using System;
using System.Collections.Generic;

namespace OrderMatchingEngine.OrderBook.Stats
{
    public class Statistics
    {
        public enum Stat
        {
            NumOrders
        }

        private readonly Dictionary<Stat, Statistic> m_Stats;

        public Statistics(IDictionary<Stat, Statistic> stats)
        {
            if (stats == null) throw new ArgumentNullException("stats");
            m_Stats = new Dictionary<Stat, Statistic>(stats);
        }

        public Statistics() : this(new Dictionary<Stat, Statistic>(){{Stat.NumOrders, new Statistic()} })
        {}

        public Statistic this[Stat stat]
        {
            get { return m_Stats[stat]; }
        }
    }
}
