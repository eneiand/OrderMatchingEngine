using System;
using System.Collections.Generic;

namespace OrderMatchingEngine.Exchange
{
    public class Statistics
    {
        private readonly Dictionary<String, Statistic> m_Stats;

        public Statistics(IDictionary<String, Statistic> stats)
        {
            if (stats == null) throw new ArgumentNullException("stats");
            m_Stats = new Dictionary<string, Statistic>(stats);
        }

        public Statistic this[String name]
        {
            get { return m_Stats[name]; }
        }
    }
}
