namespace OrderApplication;

public sealed class Order
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "";
    public string Provider { get; set; } = "";
    public string Status { get; set; } = "PENDING";
    public string? TransactionId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed record CheckoutResult(bool Success, string Message, Order? Order);
