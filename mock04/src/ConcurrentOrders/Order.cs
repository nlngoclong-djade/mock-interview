namespace ConcurrentOrders;

public sealed class Order
{
    public string Id { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "PENDING";
    public string? ChargeId { get; set; }
}

public sealed record ProcessResult(bool Success, string Message, Order? Order);
