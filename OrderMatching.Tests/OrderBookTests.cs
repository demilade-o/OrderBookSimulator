namespace OrderMatching.Tests;

public class OrderBookTests
{
    [Fact]
    public void NoMatch_WhenBelowSellPrice()
    {
        var book = new OrderBook();
        book.AddOrder(new(Side.Buy, 100m, 10));
        book.AddOrder(new(Side.Sell, 105m, 10));

        var (trades, remaining) = book.Match();

        Assert.Empty(trades);
        Assert.Equal(2, remaining.Count());
    }

    [Fact]
    public void ExactMatch_ExecutesSingleTrade()
    {
        var book = new OrderBook();
        var buy = new Order(Side.Buy, 101m, 5);
        var sell = new Order(Side.Sell, 100m, 5);
        
        book.AddOrder(buy);
        book.AddOrder(sell);

        var (trades, remaining) = book.Match();

        Assert.Single(trades);
        Assert.Equal(5, trades[0].qty);
        Assert.Empty(remaining);
    }
    
    [Fact]
    public void PartialFill_LeavesRemainingQuantity()
    {
        var book = new OrderBook();
        var buy = new Order(Side.Buy, 102m, 3);
        var sell = new Order(Side.Sell, 100m, 5);
        
        book.AddOrder(buy);
        book.AddOrder(sell);

        var (trades, remaining) = book.Match();

        Assert.Single(trades);
        Assert.Equal(3, trades[0].qty);
        Assert.Single(remaining);
        Assert.Equal(2, remaining.First().Quantity);
    }
    
    [Fact]
    public void PriceTimePriority_OrdersMatchIncorrectSequence()
    {
        var book = new OrderBook();
        var buy1 = new Order(Side.Buy, 100m, 5);
        Thread.Sleep(1);
        var buy2 = new Order(Side.Buy, 100m, 5);
        var sell = new Order(Side.Sell, 100m, 8);
        
        book.AddOrder(buy1);
        book.AddOrder(buy2);
        book.AddOrder(sell);

        var (trades, remaining) = book.Match();

        Assert.Equal(2, trades.Count);
        Assert.Equal(buy1.Id, trades[0].buy.Id);
        Assert.Equal(5, trades[0].qty);
        Assert.Equal(buy2.Id, trades[1].buy.Id);
        Assert.Equal(3, trades[1].qty);
    }
    
}