using System.Collections.Concurrent;
using System.Threading;

namespace OrderMatchingEngine.OrderBook.OrderProcessing
{
    public class DedicatedThreadsOrderProcessor : OrderProcessor
    {
        private readonly Thread m_Thread;
        private readonly BlockingCollection<Order> m_PendingOrders = new BlockingCollection<Order>();

        public DedicatedThreadsOrderProcessor(BuyOrders buyOrders, SellOrders sellOrders, Trades trades)
            : base(buyOrders, sellOrders, trades)
        {
            m_Thread = new Thread(ProcessOrders);
            m_Thread.Start();
        }


        private void ProcessOrders()
        {
            foreach (Order order in m_PendingOrders.GetConsumingEnumerable())
                ProcessOrder(order);
        }

        public void Stop()
        {
            m_PendingOrders.CompleteAdding();
        }

        public override void InsertOrder(Order order)
        {
            m_PendingOrders.Add(order);
        }
    }
}