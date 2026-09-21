namespace LegacyPayments;

public interface IPaymentDataStore
{
    Payment? Load(string id);
    void Save(Payment payment);
}

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

public class PaymentRepository
{
    private readonly IPaymentDataStore _store;

    public PaymentRepository(IPaymentDataStore store)
    {
        _store = store;
    }

    public Payment? Get(string id)
    {
        return _store.Load(id);
    }

    public void Save(Payment payment)
    {
        _store.Save(payment);
    }
}
