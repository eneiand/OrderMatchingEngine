using System;
using System.Collections;
using System.Collections.Generic;

namespace OrderMatchingEngine.OrderBook
{
    public abstract class Orders : IEnumerable<Order>
    {
        private readonly List<Order> m_Orders = new List<Order>();
        private readonly Object m_Locker = new object();
        private readonly Comparison<Order> m_OrderSorter;
        private readonly Instrument m_Instrument;

        protected Orders(Instrument instrument, Comparison<Order> orderSorter)
        {
            if (instrument == null) throw new ArgumentNullException("instrument");
            if (orderSorter == null) throw new ArgumentNullException("orderSorter");

            m_OrderSorter = orderSorter;
            m_Instrument = instrument;
        }

        public  void Insert(Order order)
        {
            if (order == null) throw new ArgumentNullException("order");
            if(!OrderIsForThisList(order)) throw new ArgumentException("order is not valid for this Orders instance", "order");


            lock (m_Locker)
            {
                m_Orders.Add(order);
                m_Orders.Sort(m_OrderSorter);
            }
        }

        private bool OrderIsForThisList(Order order)
        {
            return order.Instrument == this.m_Instrument && OrderIsCorrectType(order);
        }

        protected abstract bool OrderIsCorrectType(Order order);

        protected static int EarliestTimeFirst(Order x, Order y)
        {
            return  x.CreationTime.CompareTo(y.CreationTime);
        }

        protected static int HighestPriceFirst(Order x, Order y)
        {
            return -1 * x.Price.CompareTo(y.Price);
        }

        public IEnumerator<Order> GetEnumerator()
        {
            List<Order> ordersCopy;
            lock(m_Locker)
            {
                ordersCopy = new List<Order>(m_Orders);
            }
            return ordersCopy.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class BuyOrders : Orders
    {
        private static int HighestPriceEarliestTimePriority(Order x, Order y)
        {
                int priceComp = HighestPriceFirst(x, y);

                if (priceComp == 0)
                    return EarliestTimeFirst(x, y);
            
                return priceComp;
        }


        public BuyOrders(Instrument instrument) : base(instrument, HighestPriceEarliestTimePriority) { }

        protected override bool OrderIsCorrectType(Order order)
        {
            return order.BuySell == Order.BuyOrSell.Buy;
        }
    }

    public class SellOrders : Orders
    {
        private static int LowestPriceEarliestTimePriority(Order x, Order y)
        {
            int priceComp = (-1 * HighestPriceFirst(x, y));

            if (priceComp == 0)
                return EarliestTimeFirst(x, y);
            
            return priceComp;
        }


        public SellOrders(Instrument instrument) : base(instrument, LowestPriceEarliestTimePriority) { }

        protected override bool OrderIsCorrectType(Order order)
        {
            return order.BuySell == Order.BuyOrSell.Sell;
        }
    }
}
