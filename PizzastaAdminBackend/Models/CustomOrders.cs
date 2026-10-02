namespace PizzastaAdminBackend.Models
{
    public class CustomOrders
    {
        public Guid Id { get; set; }
        public string OrderType { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Crust { get; set; } = string.Empty;
        public string Sauce { get; set; } = string.Empty;
        public string Toppings { get; set; } = string.Empty;
        public int OrderQuantity { get; set; } = 1;
        public int TotalPrice { get; set; } = 0;
        public DateTime OrderPlacedAt { get; set; } = DateTime.Now;
        public string OrderStatus { get; set; } = "Pending";
        public string OrderBy { get; set; } = string.Empty;
        public string OrderByNumber { get; set; } = string.Empty;
    }
}
