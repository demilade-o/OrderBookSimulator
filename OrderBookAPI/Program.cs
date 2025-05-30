using OrderBookAPI.Models;
using OrderMatching;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<OrderBook>();

var app = builder.Build();

app.MapPost("/orders", (OrderDTO dto, OrderBook book) =>
{
    if (Enum.TryParse<Side>(dto.Side, true, out var side)) 
        return Results.BadRequest($"Invalid Side: {dto.Side}");

    var order = new Order(side, dto.Price, dto.Quantity);
    book.AddOrder(order);

    var (trades, _) = book.Match();

    var result = trades.Select(t => new
    {
        BuyId = t.buy.Id,
        SellId = t.sell.Id,
        Quantity = t.qty,
        Price = t.sell.Price
    });
    
    return Results.Ok(result);
});