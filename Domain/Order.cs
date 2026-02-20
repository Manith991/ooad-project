namespace OOAD_Project.Domain
{
    public class Order
    {
        public int OrderId { get; set; }
        public int? TableId { get; set; }
        public int? UserId { get; set; }
        public string OrderType { get; set; } = "Dine-in";
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public string PaymentMethod { get; set; } = "Cash";

        public string? TableName { get; set; }
        public string? StaffName { get; set; }

        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public Order() { }

        public Order(int orderId, int? tableId, int userId, string orderType,
            decimal totalAmount, string status, string paymentMethod)
        {
            OrderId = orderId;
            TableId = tableId;
            UserId = userId;
            OrderType = orderType;
            TotalAmount = totalAmount;
            Status = status;
            PaymentMethod = paymentMethod;
        }
    }
}
