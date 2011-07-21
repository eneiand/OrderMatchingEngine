using System;
using System.Collections.Generic;

namespace OrderMatchingEngine.OrderBook.OrderProcessing
{
    public abstract class OrderProcessor
    {
        public delegate bool OrderMatcher(Order order, Orders orders, Trades trades);

        public static bool TryMatchOrder(Order order, Orders orders, Trades trades)
        {
            List<Order> candidateOrders = order.BuySell == Order.BuyOrSell.Buy
                                              ? new List<Order>(orders.FindAll(o => o.Price <= order.Price))
                                              : new List<Order>(orders.FindAll(o => o.Price >= order.Price));
            if (candidateOrders.Count == 0)
                return false;

            foreach (Order candidateOrder in candidateOrders)
            {
                //once an order has been filled completely our job is done
                if (order.Quantity == 0)
                    break;

                ulong quantity = (candidateOrder.Quantity >= order.Quantity
                                      ? order.Quantity
                                      : candidateOrder.Quantity);

                candidateOrder.Quantity -= quantity;
                order.Quantity -= quantity;

                if (candidateOrder.Quantity == 0)
                    orders.Remove(candidateOrder);

                trades.AddTrade(new Trade(order.Instrument, quantity, candidateOrder.Price));
            }
            return true;
        }

        protected BuyOrders m_BuyOrders;
        protected SellOrders m_SellOrders;
        protected Trades m_Trades;

        public OrderMatcher TryMatchBuyOrder { get; set; }
        public OrderMatcher TryMatchSellOrder { get; set; }


        public OrderProcessor(BuyOrders buyOrders, SellOrders sellOrders, Trades trades,
                              OrderMatcher tryMatchBuyOrder, OrderMatcher tryMatchSellOrder)
        {
            m_BuyOrders = buyOrders;
            m_SellOrders = sellOrders;
            m_Trades = trades;
            TryMatchBuyOrder = tryMatchBuyOrder;
            TryMatchSellOrder = tryMatchSellOrder;
        }

        public OrderProcessor(BuyOrders buyOrders, SellOrders sellOrders, Trades trades)
            : this(buyOrders, sellOrders, trades,
                   TryMatchOrder,
                   TryMatchOrder)
        {
        }


        public abstract void InsertOrder(Order order);

        protected void ProcessOrder(Order order)
        {
            switch (order.BuySell)
            {
                case Order.BuyOrSell.Buy:
                    TryMatchBuyOrder(order, m_SellOrders, m_Trades);
                    if (order.Quantity > 0)
                        m_BuyOrders.Insert(order);
                    break;
                case Order.BuyOrSell.Sell:
                    TryMatchSellOrder(order, m_BuyOrders, m_Trades);
                    if (order.Quantity > 0)
                        m_SellOrders.Insert(order);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}