using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using OrderBookAPI.Models;

namespace OrderBookAPI.Tests;

public class OrderBookAPITests :
    IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrderBookAPITests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostOrder_ReturnsTrades_AndBookUpdates()
    {
        var dto1 = new OrderDto("Buy", 100m, 5);
        var dto2 = new OrderDto("Sell", 99m, 3);

        var resp1 = await _client.PostAsJsonAsync("/orders", dto1);
        resp1.StatusCode.Should().Be(HttpStatusCode.OK);
        var trades1 = await resp1.Content.ReadFromJsonAsync<object[]>();
        trades1.Should().BeEmpty();
        
        var resp2 = await _client.PostAsJsonAsync("/orders", dto2);
        resp2.StatusCode.Should().Be(HttpStatusCode.OK);
        var trades2 = await resp2.Content.ReadFromJsonAsync<TradeDto[]>();

        trades2.Should().HaveCount(1);
        trades2[0].Quantity.Should().Be(3);
        trades2[0].Price.Should().Be(99m);

        var bookResp = await _client.GetFromJsonAsync<BookDto>("/book");
        bookResp.Buys.Should().ContainSingle().Which.Quantity.Should().Be(2);
    }
    
    record TradeDto(string BuyId, string sellId, int Quantity, decimal Price);
}

public class BookEntryDto
{
    public string   Id       { get; set; }
    public decimal  Price    { get; set; }
    public int      Quantity { get; set; }
}

public class BookDto
{
    public BookEntryDto[] Buys  { get; set; }
    public BookEntryDto[] Sells { get; set; }
}
