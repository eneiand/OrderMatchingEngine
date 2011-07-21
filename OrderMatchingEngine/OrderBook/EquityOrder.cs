using System;

namespace OrderMatchingEngine.OrderBook
{
    public class EquityOrder : Order
    {
        public EquityOrder(Instrument instrument, OrderTypes orderType, BuyOrSell buySell, Decimal price,
                           UInt64 quantity)
            : base(instrument, orderType, buySell, price, quantity)
        {
        }
    }
}
