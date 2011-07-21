using System;
using System.Collections.Generic;
using System.IO;

namespace OrderMatchingEngine.OrderBook
{
    public class Trades
    {
        public Instrument Instrument { get; private set; }
        public TradeProcessor TradeProcessingStrategy { get; set; }
        private readonly Object m_Locker = new Object();

        public Trades(Instrument instrument, TradeProcessor tradeProcessingStrategy)
        {
            Instrument = instrument;
            TradeProcessingStrategy = tradeProcessingStrategy;
        }

        public Trades(Instrument instrument)
            : this(instrument, new InMemoryTradeProcessor())
        {
        }

        public void AddTrade(Trade trade)
        {
            if (trade == null) throw new ArgumentNullException("trade");

            if (trade.Instrument != Instrument)
                throw new TradeIsNotForThisInstrumentException();

            lock (m_Locker)
                TradeProcessingStrategy.Add(trade);
        }

        public class TradeIsNotForThisInstrumentException : Exception
        {
        }

        public abstract class TradeProcessor
        {
            public abstract void Add(Trade trade);
        }

        public class StreamWritingTradeProcessor : TradeProcessor
        {
            public StreamWriter Writer { get; private set; }

            public StreamWritingTradeProcessor(StreamWriter writer)
            {
                Writer = writer;
            }

            public override void Add(Trade trade)
            {
                Writer.Write(trade.ToString());
            }
        }

        public class InMemoryTradeProcessor : TradeProcessor
        {
            private readonly List<Trade> m_Trades = new List<Trade>();
            private readonly Object m_Locker = new object();

            public override void Add(Trade trade)
            {
                lock (m_Locker)
                    m_Trades.Add(trade);
            }

            public List<Trade> Trades
            {
                get
                {
                    var tradesCopy = new List<Trade>(m_Trades);
                    return tradesCopy;
                }
            }
        }
    }
}
