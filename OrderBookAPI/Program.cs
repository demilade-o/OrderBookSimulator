using OrderBookAPI.Models;
using OrderMatching;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<OrderBook>();

var app = builder.Build();

app.MapPost("/orders", (OrderDto dto, OrderBook book) =>
{
    if (!Enum.TryParse<Side>(dto.Side, true, out var side))
    {
        return Results.BadRequest($"Invalid Side: {dto.Side}");
    }

    var order = new Order(side, dto.Price, dto.Quantity);
    book.AddOrder(order);

    var (trades, _) = book.Match();

    var result = trades.Select(t => new
    {
        BuyId = t.buy.Id,
        SellId = t.sell.Id,
        Quantity = t.qty,
        Price = t.sell.Price,
    });
    return Results.Ok(result);
});

app.MapGet("/book", (OrderBook book) =>
{
    var buys = book.GetTopBuys(5).Select(o => new {o.Id, o.Price, o.Quantity});
    var sells = book.GetTopSells(5).Select(o => new {o.Id, o.Price, o.Quantity});
    return Results.Ok(new { Buys = buys, Sells = sells });
});

app.Run();

public partial class Program { }
