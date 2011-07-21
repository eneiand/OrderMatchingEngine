namespace OrderMatchingEngine.OrderBook.OrderProcessing
{
    public class SynchronousOrderProcessor : OrderProcessor
    {
        public SynchronousOrderProcessor(BuyOrders buyOrders, SellOrders sellOrders, Trades trades)
            : base(buyOrders, sellOrders, trades)
        {
        }

        public override void InsertOrder(Order order)
        {
            ProcessOrder(order);
        }
    }
}