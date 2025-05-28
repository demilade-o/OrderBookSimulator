using System;
using System.Collections.Concurrent;
using OrderMatching;

namespace CoreEngine;

class Program
{
    static void ProduceRandomOrders(BlockingCollection<Order> queue, CancellationToken token)
    {
        var rnd = new Random();

        try
        {
            while (!token.IsCancellationRequested)
            {
                var side = rnd.Next(2) == 0 ? Side.Buy : Side.Sell;
                var price = Math.Round((decimal)(rnd.NextDouble() * 10 + 100), 2);
                var qty = rnd.Next(1, 21);
                var order = new Order(side, price, qty);
                
                queue.Add(order, token);
                Console.WriteLine($"Enqueued {order}");
                
                Thread.Sleep(50);
            }
        }
        catch (OperationCanceledException){}
    }
}