using System;
using System.Threading;

namespace OrderMatchingEngine.OrderBook.Stats
{
    public class Statistic
    {
        private long m_Value = 0;
        private readonly Object m_Locker = new Object();

        public long Value { get { lock (m_Locker) return m_Value; }}

        public void Reset()
        {
            lock (m_Locker)
                m_Value = 0;
        }

        public static Statistic operator++(Statistic stat)
        {
            Interlocked.Increment(ref stat.m_Value);
            return stat;
        }
    }
}
