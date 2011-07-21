using System;
using System.Threading;

namespace OrderMatchingEngine.Exchange
{
    public class Statistic
    {
        private long m_Value = 0;
        private readonly Object m_Locker = new Object();

        public Statistic(String name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Name = name;
        }

        public String Name { get; private set; }
        public long Value { get { lock (m_Locker) return m_Value; }}

        public static Statistic operator++(Statistic stat)
        {
            Interlocked.Increment(ref stat.m_Value);
            return stat;
        }
    }
}
