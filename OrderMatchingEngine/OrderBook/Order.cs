using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OrderMatchingEngine
{
    public abstract class Order
    {
        public enum BuyOrSell
        {
            Buy,
            Sell
        }

        public enum OrderTypes
        {
            GoodUntilCancelled,
            GoodUntilDate,
            ImmediateOrCancel,
            LimitPrice,
            MarketPrice,
            StopLoss
        }

        private static Int64 GlobalOrderId;
        private UInt64 m_Quantity;
        private Object m_Locker;

        protected Order()
        {
            Id = Interlocked.Increment(ref GlobalOrderId);
            CreationTime = DateTime.Now;
        }
        protected Order(Instrument instrument, OrderTypes orderType, BuyOrSell buySell, Decimal price, UInt64 quantity)
            : this()
        {
            if (instrument == null) throw new ArgumentNullException("instrument");

            Instrument = instrument;
            OrderType = orderType;
            BuySell = buySell;
            Price = price;
            Quantity = quantity;

        }

        public BuyOrSell BuySell { get; private set; }
        public OrderTypes OrderType { get; private set; }
        public Decimal Price { get; private set; }
        public UInt64 Quantity { get { lock (m_Locker) return m_Quantity; } set { lock (m_Locker) m_Quantity = value; } }
        public Instrument Instrument { get; private set; }
        public DateTime CreationTime { get; private set; }
        public Int64 Id { get; private set; }



    }
}
