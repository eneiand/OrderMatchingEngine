using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrderMatchingEngine.Exchange
{
    public class Market
    {
         
        private readonly Dictionary<Instrument, OrderBook.OrderBook> m_OrderBooks;

        public Market(IDictionary<Instrument, OrderBook.OrderBook> orderBooks)
        {
            if (orderBooks == null) throw new ArgumentNullException("orderBooks");

            m_OrderBooks = new Dictionary<Instrument, OrderBook.OrderBook>(orderBooks);
        }

        public OrderBook.OrderBook this[Instrument instrument]
        {
            get
            {
                if(instrument == null) throw new ArgumentNullException("instrument");

                OrderBook.OrderBook orderBook;

                if (this.m_OrderBooks.TryGetValue(instrument, out orderBook))
                {
                    return orderBook;
                }
                else
                {
                    throw new InstrumentNotInThisMarketException();
                }
            }
        }

        public void SubmitOrder(Order order)
        {
            if (order == null) throw new ArgumentNullException("order");

            OrderBook.OrderBook orderBook = this[order.Instrument];
            orderBook.InsertOrder(order);
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
