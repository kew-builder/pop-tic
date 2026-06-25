namespace PopTic.Api.Models;

public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public string CustomerEmail { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "pending"; // pending | paid | expired
    public string? ChargeId { get; set; }
    public List<Ticket> Tickets { get; set; } = [];
}
