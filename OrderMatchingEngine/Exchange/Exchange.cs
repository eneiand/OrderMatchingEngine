using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OrderMatchingEngine.Exchange
{
    class Exchange
    {
        private Dictionary<Market.MarketName, Market> m_Markets;

        public Exchange(IDictionary<Market.MarketName, Market> markets)
        {
            if (markets == null) throw new ArgumentNullException("markets");

            m_Markets = new Dictionary<Market.MarketName, Market>(markets);
        }

        public Market this[Market.MarketName marketName]
        {
            get
            {
                if (marketName == null) throw new ArgumentNullException("marketName");

                Market market;

                if (this.m_Markets.TryGetValue(marketName, out market))
                    return market;
                else
                    throw new MarketNotOnThisExchangeException();
            }
        }

        public class MarketNotOnThisExchangeException : Exception
        {
            public MarketNotOnThisExchangeException()
                : base("market is not on this exchange")
            {
            }
        }
    }
}
