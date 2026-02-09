namespace OOAD_Project.Domain
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal => Quantity * Price;

        public string ProductName { get; set; } = string.Empty;

        public OrderDetail() { }

        public OrderDetail(int productId, string productName, int quantity, decimal price)
        {
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            Price = price;
        }
    }
}
