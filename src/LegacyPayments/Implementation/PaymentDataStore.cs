using LegacyPayments.Interface;

namespace LegacyPayments.Implementation;
public sealed class InMemoryPaymentDataStore : IPaymentDataStore
{
    private readonly Dictionary<string, Payment> _payments = new();
    private readonly LruCache<string, Payment>  _cache = new(10);
    public int LoadCount { get; private set; }
    public int SaveCount { get; private set; }

    public Payment? Load(string id)
    {
        LoadCount++;
        var key = "payment-" + id;
        if (!_cache.TryGet(key, out var cacheValue))
        {
            if (_payments.TryGetValue(id, out var payment))
            {
                _cache.Put(key, Copy(payment));
                return _cache.TryGet(key, out var result) ? result : null;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return cacheValue;
        }
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

