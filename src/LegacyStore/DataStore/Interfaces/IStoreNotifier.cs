namespace LegacyStore.DataStore.Interfaces;

public interface IStoreNotifier
{
    void Send(string recipient, string subject, string body);
}