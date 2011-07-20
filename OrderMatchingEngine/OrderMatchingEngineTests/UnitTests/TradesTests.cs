using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OrderMatchingEngine;
using OrderMatchingEngine.OrderBook;

namespace OrderMatchingEngineTests.UnitTests
{
    [TestFixture]
    class TradesTests
    {
        private Trades m_Trades;
        private Instrument m_Instrument;

        [SetUp]
        public void Init()
        {
            m_Instrument = new Instrument("MSFT");
            m_Trades = new Trades(m_Instrument);
        }

        [Test]
        public void AddTradeTest()
        {
            var trade = new Trade(m_Instrument, 100UL, 100.10M);
            m_Trades.AddTrade(trade);
            
            Assert.That(((Trades.InMemoryTradeProcessor)m_Trades.TradeProcessingStrategy).Trades[0], Is.EqualTo(trade));
        }
    }
}
