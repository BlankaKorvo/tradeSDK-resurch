namespace MarketDataModules
{ 
    public class OrderbookEntry
    {
        public int Quantity { get; }
        public decimal Price { get; }

        public OrderbookEntry(int quantity, decimal price)
        {
            Quantity = quantity;
            Price = price;
        }
    }
}