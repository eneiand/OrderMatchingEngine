using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OrderMatchingEngine
{
    public class Trade
    {
        private static Int64 GlobalTradeId;

        public Trade(Instrument instrument, UInt64 quantity, Decimal price)
            : this()
        {
            if (instrument == null) throw new ArgumentNullException("instrument");

            Instrument = instrument;
            Quantity = quantity;
            Price = price;
        }

        private Trade()
        {
            Id = Interlocked.Increment(ref GlobalTradeId);
            CreationTime = DateTime.Now;
        }

        public Instrument Instrument { get; private set; }
        public UInt64 Quantity { get; private set; }
        public Decimal Price { get; private set; }
        public Int64 Id { get; private set; }
        public DateTime CreationTime { get; private set; }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder(Instrument.Symbol);
            s.AppendFormat(" {0} {1} ", this.Quantity, this.Price);
            return s.ToString();
        }
    }
}
