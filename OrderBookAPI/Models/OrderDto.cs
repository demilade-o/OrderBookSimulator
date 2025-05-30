namespace OrderBookAPI.Models;

public record OrderDto(String Side, decimal Price, int Quantity);
