namespace ShoeStoreAPI.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public string Status { get; set; } = "pending";
    public decimal TotalAmount { get; set; }
    public string? CustomerName { get; set; } // для JOIN запросов
}
