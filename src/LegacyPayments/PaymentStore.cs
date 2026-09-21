namespace LegacyPayments;

public interface IPaymentStore
{
    Payment? Load(string id);
    void Save(Payment payment);
}

public sealed class InMemoryPaymentStore : IPaymentStore
{
    private readonly Dictionary<string, Payment> _payments = new(StringComparer.OrdinalIgnoreCase);

    public int LoadCount { get; private set; }
    public int SaveCount { get; private set; }
    public bool FailNextSave { get; set; }

    public Payment? Load(string id)
    {
        LoadCount++;
        return _payments.TryGetValue(id, out var payment) ? Copy(payment) : null;
    }

    public void Save(Payment payment)
    {
        SaveCount++;

        if (FailNextSave)
        {
            FailNextSave = false;
            throw new InvalidOperationException("Simulated persistence failure");
        }

        _payments[payment.Id] = Copy(payment);
    }

    private static Payment Copy(Payment payment) => new()
    {
        Id = payment.Id,
        CustomerEmail = payment.CustomerEmail,
        Amount = payment.Amount,
        Currency = payment.Currency,
        Status = payment.Status,
        ProcessingFee = payment.ProcessingFee,
        UpdatedAtUtc = payment.UpdatedAtUtc,
        AuditTrail = new List<string>(payment.AuditTrail)
    };
}

public class PaymentRepository
{
    private readonly IPaymentStore _store;

    public PaymentRepository(IPaymentStore store)
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
