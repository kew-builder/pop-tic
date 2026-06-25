namespace PopTic.Api.Models;

public class Ticket
{
    public int Id { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public Order Order { get; set; } = null!;
    public string TicketCode { get; set; } = string.Empty;
}
