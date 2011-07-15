using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrderMatchingEngine.OrderBook
{
    public abstract class Orders
    {
        private List<Order> m_Orders = new List<Order>();
        private Object m_Locker = new object();
        private Comparison<Order> m_OrderSorter;

        protected Orders(Comparison<Order> orderSorter)
        {
            m_OrderSorter = orderSorter;
        }

        public void Insert(Order order)
        {
            lock (m_Locker)
            {
                m_Orders.Add(order);
                m_Orders.Sort(m_OrderSorter);
            }
        }

    }

    public class BuyOrders : Orders
    {
        private static int PriceTimePriority(Order x, Order y) 
            {
                int priceComp = x.Price.CompareTo(y.Price);
                
                if(priceComp == 0)
                    return -1 * x.CreationTime.CompareTo(y.CreationTime);
                else
                    return priceComp;
            }


        public BuyOrders() : base(PriceTimePriority) { }
    }

    public class SellOrders : Orders
    {
        private static int PriceTimePriority(Order x, Order y)
        {
            int priceComp = -1 * x.Price.CompareTo(y.Price);

            if (priceComp == 0)
                return -1 * x.CreationTime.CompareTo(y.CreationTime);
            else
                return priceComp;
        }


        public SellOrders() : base(PriceTimePriority) { }
    }
}
