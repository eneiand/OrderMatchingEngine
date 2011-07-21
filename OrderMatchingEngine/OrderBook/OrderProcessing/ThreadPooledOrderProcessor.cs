using System.Threading;

namespace OrderMatchingEngine.OrderBook.OrderProcessing
{
    public class ThreadPooledOrderProcessor : OrderProcessor
    {
        public ThreadPooledOrderProcessor(BuyOrders buyOrders, SellOrders sellOrders, Trades trades)
            : base(buyOrders, sellOrders, trades)
        {
        }

        public override void InsertOrder(Order order)
        {
            ThreadPool.QueueUserWorkItem((o) => ProcessOrder(order));
        }
    }
}