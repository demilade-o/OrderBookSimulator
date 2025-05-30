using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using OrderMatching;

namespace CoreEngine;

class Program
{
    static async Task Main(string[] args)
    {
        var channel = Channel.CreateBounded<Order>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = false,
            SingleReader = true
        });
        
        var book = new OrderBook();
        var cts = new CancellationTokenSource();
        
        var consumer = ConsumeOrdersAsync(channel.Reader, book, cts.Token);
        var producers = ProduceRandomOrdersAsync(channel.Writer, cts.Token);
        
        Console.WriteLine("Press ENTER to stop");
        Console.ReadLine();
        
        cts.Cancel();
        channel.Writer.Complete();

        await producers;
        await consumer;
        
        Console.WriteLine("All orders processed. Shutting down...");
    }
    
    static async Task ProduceRandomOrdersAsync(ChannelWriter<Order> writer, CancellationToken token)
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
                
                await writer.WriteAsync(order, token);
                Console.WriteLine($"Enqueued {order}");
                
                await Task.Delay(50, token);
            }
        }
        catch (OperationCanceledException){}
    }

    static async Task ConsumeOrdersAsync(ChannelReader<Order> reader, OrderBook book, CancellationToken token)
    {
        await foreach (var order in reader.ReadAllAsync(token))
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