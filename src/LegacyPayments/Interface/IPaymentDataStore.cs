namespace LegacyPayments.Interface;

public interface IPaymentDataStore
{
    Payment? Load(string id);
    void Save(Payment payment);
}