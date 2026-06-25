namespace PopTic.Api.Models;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
}
