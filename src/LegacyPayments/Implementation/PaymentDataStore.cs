using LegacyPayments.Interface;

namespace LegacyPayments.Implementation;
public sealed class InMemoryPaymentDataStore : IPaymentDataStore
{
    private readonly Dictionary<string, Payment> _payments = new();

    public int LoadCount { get; private set; }
    public int SaveCount { get; private set; }

    public Payment? Load(string id)
    {
        LoadCount++;
        return _payments.TryGetValue(id, out var payment) ? Copy(payment) : null;
    }

    public void Save(Payment payment)
    {
        SaveCount++;
        _payments[payment.Id] = Copy(payment);
    }

    private static Payment Copy(Payment payment) => new()
    {
        Id = payment.Id,
        CustomerEmail = payment.CustomerEmail,
        Amount = payment.Amount,
        Currency = payment.Currency,
        Status = payment.Status,
        Fee = payment.Fee,
        UpdatedAtUtc = payment.UpdatedAtUtc
    };
}

