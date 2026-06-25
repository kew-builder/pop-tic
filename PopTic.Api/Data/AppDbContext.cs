using Microsoft.EntityFrameworkCore;
using PopTic.Api.Models;

namespace PopTic.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Event)
            .WithMany()
            .HasForeignKey(o => o.EventId);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Order)
            .WithMany(o => o.Tickets)
            .HasForeignKey(t => t.OrderId);

        modelBuilder.Entity<Event>().HasData(new Event
        {
            Id = 1,
            Title = "Tech Conference 2026",
            Price = 250,
            TotalSeats = 500,
            AvailableSeats = 500,
        });
    }
}
