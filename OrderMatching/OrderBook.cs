using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderMatching;

public class OrderBook
{
    private readonly SortedSet<Order> _buys = new(
        Comparer<Order>.Create((a, b) =>
            a.Price != b.Price
                ? b.Price.CompareTo(a.Price)
                : a.TimeStamp.CompareTo(b.TimeStamp)));
    
    private readonly SortedSet<Order> _sells = new(
        Comparer<Order>.Create((a, b) =>
            a.Price != b.Price
                ? a.Price.CompareTo(b.Price)
                : a.TimeStamp.CompareTo(b.TimeStamp)));
    
    private readonly object _lock = new ();

    public void AddOrder(Order o)
    {
        lock (_lock)
        {
            if (o.Side == Side.Buy)
            {
                _buys.Add(o);
            }
            else
            {
                _sells.Add(o);
            }
        }
    }

    public (List<(Order buy, Order sell, int qty)> Trades, List<Order> Remaining) Match()
    {
        var trades = new List<(Order, Order, int)>();

        lock (_lock)
        {
            while (_buys.Any() && _sells.Any() && _buys.First().Price >= _sells.First().Price)
            {
                var buy = _buys.First();
                var sell = _sells.First();
                
                int qty = Math.Min(buy.Quantity, sell.Quantity);
                
                buy.Quantity -= qty;
                sell.Quantity -= qty;

                trades.Add((buy, sell, qty));

                if (buy.Quantity == 0)
                {
                    _buys.Remove(buy);
                }

                if (sell.Quantity == 0)
                {
                    _sells.Remove(sell);
                }
            }

            var remaining = _buys.Concat(_sells).ToList();

            return (trades, remaining);
        }
    }

    public override string ToString()
    {
        lock (_lock)
        {
            var buyLines = _buys.Take(5).Select(o => o.ToString());
            var sellLines = _sells.Take(5).Select(o => o.ToString());

            return $"--- BUY SIDE ---\n" +
                   $"{string.Join("\n", buyLines)}\n\n" +
                   $"--- SELL SIDE ---\n" +
                   $"{string.Join("\n", sellLines)}";
        }
    }
}