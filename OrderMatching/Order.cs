using System;

namespace OrderMatching;
    public enum Side {Buy, Sell}

    public class Order
    {
        public Guid Id { get; } = Guid.NewGuid();
        
        public Side Side { get; }
        
        public decimal Price { get; }
        
        public int Quantity { get; set; }

        public DateTime TimeStamp { get; } = DateTime.UtcNow;

        public Order(Side side, decimal price, int qty)
        {
            Side = side;
            Price = price;
            Quantity = qty;
        }

        public override string ToString() => $"{Side} {Price}@{Quantity} ({Id.ToString()[..8]})";
    }
