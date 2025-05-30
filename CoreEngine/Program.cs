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
        
        Console.WriteLine("Welcome to OrderBook Simulator!");
        Console.WriteLine("Commands:");
        Console.WriteLine("    BUY <price> <quantity>    e.g BUY 101.50 10");
        Console.WriteLine("    SELL <price> <quantity>   e.g BUY 100.00 5");
        Console.WriteLine("    BOOK                      Show top of book");
        Console.WriteLine("    EXIT                      Shut down");
        Console.WriteLine();

        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line is null) continue;

            var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;
            
            var cmd = parts[0].ToUpperInvariant();
            if (cmd == "EXIT")
            {
                Console.WriteLine("Shutting down input...");
                break;
            }
            if (cmd == "BOOK")
            {
                Console.WriteLine(book);
            }

            else if ((cmd == "BUY" || cmd == "SELL") && parts.Length == 3)
            {
                if (!decimal.TryParse(parts[1], out var price))
                {
                    Console.WriteLine("Invalid price");
                    continue;
                }
                if (!int.TryParse(parts[2], out var qty))
                {
                    Console.WriteLine("Invalid quantity. Usage: BUY <price> <quantity>");
                    continue;
                }

                var side = cmd == "BUY" ? Side.Buy : Side.Sell;
                var order = new Order(side, price, qty);

                try
                {
                    orderQueue.Add(order, cts.Token);
                    Console.WriteLine($"Enqueued {order}");
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Input cancelled. Cannot enqueue more orders.");
                    break;
                }
            }


        }
        orderQueue.CompleteAdding();
        cts.Cancel();

        await consumer;
        
        Console.WriteLine("Goodbye!");
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