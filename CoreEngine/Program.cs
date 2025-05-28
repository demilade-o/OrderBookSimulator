using System;
using System.Collections.Concurrent;
using OrderMatching;

namespace CoreEngine;

class Program
{
    static async Task Main(string[] args)
    {
        var orderQueue = new BlockingCollection<Order>(boundedCapacity:1000);
        var book = new OrderBook();
        
        var cts = new CancellationTokenSource();
        
        var consumer = Task.Run(() => ConsumeOrders(orderQueue, book, cts.Token), cts.Token);
        
        var producers = new List<Task>
        {
            Task.Run(() => ProduceRandomOrders(orderQueue, cts.Token), cts.Token),
        };
        
        Console.WriteLine("Press ENTER to stop");
        Console.ReadLine();
        
        cts.Cancel();
        orderQueue.CompleteAdding();

        await Task.WhenAll(producers);
        await consumer;
        
        Console.WriteLine("All orders processed. Shutting down...");
        

    }
    
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

    static void ConsumeOrders(BlockingCollection<Order> queue, OrderBook book, CancellationToken token)
    {
        foreach (var order in queue.GetConsumingEnumerable(token))
        {
            book.AddOrder(order);
            var (trades, _) = book.Match();
            foreach (var (buy, sell, qty) in trades)
            {
                Console.WriteLine($"TRADE: {qty} @ {sell.Price} " +
                                  $"(B {buy.Id.ToString()[..8]} / S {sell.Id.ToString()[..8]})");
            }
        }
    }
}