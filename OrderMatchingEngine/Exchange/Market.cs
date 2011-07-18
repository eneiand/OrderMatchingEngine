using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrderMatchingEngine.Exchange
{
    class Market
    {
         
        private readonly ConcurrentDictionary<Instrument, OrderBook.OrderBook> m_OrderBooks;

        public Market(IEnumerable<KeyValuePair<Instrument, OrderBook.OrderBook>> orderBooks)
        {
            if (orderBooks == null) throw new ArgumentNullException("orderBooks");

            m_OrderBooks = new ConcurrentDictionary<Instrument, OrderBook.OrderBook>(orderBooks);
        }

        public void SubmitOrder(Order order)
        {
            if (order == null) throw new ArgumentNullException("order");

            OrderBook.OrderBook orderBook;

            if(this.m_OrderBooks.TryGetValue(order.Instrument, out orderBook))
            {
                orderBook.InsertOrder(order);
            }
            else
            {
                throw new InstrumentNotInThisMarketException();
            }

        }

        public class MarketName
        {
            public MarketName(String name)
            {
                if (String.IsNullOrEmpty(name.Trim())) throw new ArgumentNullException("name");

                Name = name;
            }

            public String Name { get; private set; }
        }

        public class InstrumentNotInThisMarketException : Exception
        {
            public InstrumentNotInThisMarketException(): base("instrument does not have an orderbook in this market")
            {
            }
        }

        
    }
}
