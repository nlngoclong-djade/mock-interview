namespace NotificationDispatch;

public interface IEmailClient
{
    Task<string> SendAsync(string recipient,string message,CancellationToken ct=default);
}

public interface ISmsClient
{
    Task<string> SendAsync(string recipient,string message,CancellationToken ct=default);
}

public interface IAuditStore
{
    Task SaveAsync(string recipient,string channel,string providerId,CancellationToken ct=default);
}
