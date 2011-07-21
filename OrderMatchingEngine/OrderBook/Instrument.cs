using System;

namespace OrderMatchingEngine.OrderBook
{
    [Serializable]
    public class Instrument
    {
        public String Symbol { get; private set; }

        public Instrument(String symbol)
        {
            if (String.IsNullOrEmpty(symbol)) throw new ArgumentNullException("symbol");

            Symbol = symbol;
        }
    }
}
