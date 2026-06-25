using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PopTic.Api.Data;
using PopTic.Api.Models;

var builder = WebApplication.CreateBuilder(args);

var omiseSecretKey = builder.Configuration["Omise:SecretKey"]!;

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=poptic.db"));

builder.Services.AddHttpClient("omise", c =>
{
    c.BaseAddress = new Uri("https://api.omise.co/");
    var token = Convert.ToBase64String(Encoding.ASCII.GetBytes(omiseSecretKey + ":"));
    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseDefaultFiles();
app.UseStaticFiles();

// ─── POST /api/orders ─────────────────────────────────────────────────────────
app.MapPost("/api/orders", async (CreateOrderRequest req, AppDbContext db, IHttpClientFactory httpFactory) =>
{
    var ev = await db.Events.FindAsync(req.EventId);
    if (ev is null) return Results.NotFound(new { error = "Event not found" });
    if (ev.AvailableSeats < req.Quantity)
        return Results.BadRequest(new { error = "Not enough seats available" });

    var order = new Order
    {
        Id = Guid.NewGuid().ToString(),
        EventId = req.EventId,
        CustomerEmail = req.CustomerEmail,
        Quantity = req.Quantity,
        TotalAmount = ev.Price * req.Quantity,
        Status = "pending",
    };

    ev.AvailableSeats -= req.Quantity;
    db.Orders.Add(order);
    await db.SaveChangesAsync();

    // เรียก Omise สร้าง PromptPay QR
    string? qrUrl = null;
    try
    {
        var omise = httpFactory.CreateClient("omise");
        var amountSatang = (long)(order.TotalAmount * 100);

        // Step 1: Create source
        var sourceRes = await omise.PostAsJsonAsync("sources", new
        {
            type = "promptpay",
            amount = amountSatang,
            currency = "THB",
        });
        var source = await sourceRes.Content.ReadFromJsonAsync<JsonElement>();
        var sourceId = source.GetProperty("id").GetString();

        // Step 2: Create charge
        var chargeRes = await omise.PostAsJsonAsync("charges", new
        {
            amount = amountSatang,
            currency = "THB",
            source = sourceId,
        });
        var charge = await chargeRes.Content.ReadFromJsonAsync<JsonElement>();
        var chargeId = charge.GetProperty("id").GetString();

        qrUrl = charge
            .GetProperty("source")
            .GetProperty("scannable_code")
            .GetProperty("image")
            .GetProperty("download_uri")
            .GetString();

        order.ChargeId = chargeId;
        await db.SaveChangesAsync();

        // Test Mode: auto mark_as_paid แล้ว generate tickets เลย
        if (omiseSecretKey.StartsWith("skey_test_"))
        {
            await omise.PostAsync($"charges/{chargeId}/mark_as_paid", null);
            order.Status = "paid";
            for (int i = 0; i < order.Quantity; i++)
            {
                order.Tickets.Add(new Ticket
                {
                    OrderId = order.Id,
                    TicketCode = "TK-" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
                });
            }
            await db.SaveChangesAsync();
        }
    }
    catch
    {
        // ถ้า Omise ล้มเหลว fallback เป็น mock QR ให้ยังใช้งานได้
        qrUrl = "mock-qr";
    }

    return Results.Ok(new
    {
        orderId = order.Id,
        qrUrl,
        totalAmount = order.TotalAmount,
        eventTitle = ev.Title,
    });
});

// ─── GET /api/orders/{id} ─────────────────────────────────────────────────────
app.MapGet("/api/orders/{id}", async (string id, AppDbContext db) =>
{
    var order = await db.Orders
        .Include(o => o.Tickets)
        .FirstOrDefaultAsync(o => o.Id == id);

    if (order is null) return Results.NotFound(new { error = "Order not found" });

    return Results.Ok(new
    {
        status = order.Status,
        ticketCodes = order.Tickets.Select(t => t.TicketCode).ToArray(),
    });
});

// ─── POST /api/webhook ────────────────────────────────────────────────────────
app.MapPost("/api/webhook", async (OmiseWebhookPayload payload, AppDbContext db) =>
{
    if (payload.Data?.Status != "successful") return Results.Ok();

    var order = await db.Orders
        .Include(o => o.Tickets)
        .FirstOrDefaultAsync(o => o.ChargeId == payload.Data.Id);

    if (order is null || order.Status == "paid") return Results.Ok();

    order.Status = "paid";
    for (int i = 0; i < order.Quantity; i++)
    {
        order.Tickets.Add(new Ticket
        {
            OrderId = order.Id,
            TicketCode = "TK-" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
        });
    }

    await db.SaveChangesAsync();
    return Results.Ok();
});

// ─── POST /api/dev/simulate/{orderId} ─────────────────────────────────────────
app.MapPost("/api/dev/simulate/{orderId}", async (string orderId, AppDbContext db) =>
{
    var order = await db.Orders
        .Include(o => o.Tickets)
        .FirstOrDefaultAsync(o => o.Id == orderId);

    if (order is null) return Results.NotFound(new { error = "Order not found" });
    if (order.Status == "paid") return Results.Ok(new { message = "Already paid" });

    order.Status = "paid";
    for (int i = 0; i < order.Quantity; i++)
    {
        order.Tickets.Add(new Ticket
        {
            OrderId = order.Id,
            TicketCode = "TK-" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
        });
    }

    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Simulated payment success" });
});

app.Run();

record CreateOrderRequest(int EventId, string CustomerEmail, int Quantity);
record OmiseChargeData(string Id, string Status);
record OmiseWebhookPayload(OmiseChargeData? Data);
