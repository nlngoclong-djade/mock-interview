namespace LegacyPayments;

public sealed class Payment
{
    public string Id { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal Fee { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed record PaymentResult(bool Success, string Message, Payment? Payment);
