using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrderMatchingEngine
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
