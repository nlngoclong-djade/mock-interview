using LegacyPayments.Interface;

namespace LegacyPayments.Repositories;

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