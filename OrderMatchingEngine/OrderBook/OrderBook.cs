using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OrderMatchingEngine.OrderBook
{
    public class OrderBook
    {

        private Orders m_BuyOrders;
        private Orders m_SellOrders;
        //private System.Collections.Concurrent.ConcurrentQueue<Order> m_IncomingOrders = new System.Collections.Concurrent.ConcurrentQueue<Order>();
        private Trades m_Trades = new Trades();

        private OrderProcessor m_OrderProcessor;


        public abstract class OrderProcessor
        {
            protected Orders m_BuyOrders;
            protected Orders m_SellOrders;
            //protected System.Collections.Concurrent.ConcurrentQueue<Order> m_IncomingOrders;
            protected Trades m_Trades;

            public OrderProcessor(Orders buyOrders, Orders sellOrders, Trades trades)
            {
                m_BuyOrders = buyOrders;
                m_SellOrders = sellOrders;
                m_Trades = trades;
            }

            public abstract void InsertOrder(Order order);

            protected void ProcessOrder(Order order)
            {
                Orders orders = order.BuySell == Order.BuyOrSell.Buy ? m_BuyOrders : m_SellOrders;
                orders.Insert(order);
            }
        }

        public class SynchronousOrderProcessor : OrderProcessor
        {
            public SynchronousOrderProcessor(Orders buyOrders, Orders sellOrders, Trades trades) : base(buyOrders, sellOrders, trades) { }

            public override void InsertOrder(Order order)
            {
                ProcessOrder(order);
            }
        }

        public class PooledOrderProcessor : OrderProcessor
        {
            public PooledOrderProcessor(Orders buyOrders, Orders sellOrders, Trades trades) : base(buyOrders, sellOrders, trades) { }


            public override void InsertOrder(Order order)
            {
                ThreadPool.QueueUserWorkItem((o) => {
                        ProcessOrder(order);
                    }
                );
            }
        }
    }
}
