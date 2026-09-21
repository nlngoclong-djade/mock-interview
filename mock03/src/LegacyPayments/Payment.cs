namespace LegacyPayments;

public sealed class Payment
{
    public string Id { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal ProcessingFee { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<string> AuditTrail { get; set; } = new();
}

public sealed record PaymentResult(bool Success, string Message, Payment? Payment);
