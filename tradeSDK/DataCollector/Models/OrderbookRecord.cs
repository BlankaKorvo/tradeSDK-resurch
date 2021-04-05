
namespace DataCollector.Models
{ 
    public class OrderbookRecord
    {
        public int Quantity { get; }
        public decimal Price { get; }


        public OrderbookRecord(int quantity, decimal price)
        {
            Quantity = quantity;
            Price = price;
        }
    }
}