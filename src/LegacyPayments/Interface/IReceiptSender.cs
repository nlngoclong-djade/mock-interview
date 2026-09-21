namespace LegacyPayments.Interface;

public interface IReceiptSender
{
    void Send(string email, string subject, string body);
}