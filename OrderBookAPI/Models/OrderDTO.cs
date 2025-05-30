namespace OrderBookAPI.Models;

public record OrderDTO(String Side, decimal Price, int Quantity);