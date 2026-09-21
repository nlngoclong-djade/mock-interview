namespace OrderApplication;

public interface IOrderStore
{
    Task<Order?> LoadAsync(string id, CancellationToken cancellationToken = default);
    Task SaveAsync(Order order, CancellationToken cancellationToken = default);
}

public interface IStripeClient
{
    Task<string> CreateChargeAsync(string orderId, decimal amount, string currency, CancellationToken cancellationToken = default);
}

public interface IPayPalClient
{
    Task<string> PayAsync(string orderId, decimal amount, string currency, CancellationToken cancellationToken = default);
}

public interface IEmailSender
{
    Task SendAsync(string email, string subject, string body, CancellationToken cancellationToken = default);
}

public interface IClock { DateTime UtcNow { get; } }

public sealed class SystemClock : IClock { public DateTime UtcNow => DateTime.UtcNow; }

public sealed class InMemoryOrderStore : IOrderStore
{
    private readonly Dictionary<string, Order> _items = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _sync = new();
    public int SaveCount { get; private set; }

    public Task<Order?> LoadAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync) return Task.FromResult(_items.TryGetValue(id, out var o) ? Copy(o) : null);
    }

    public Task SaveAsync(Order order, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync) { SaveCount++; _items[order.Id] = Copy(order); }
        return Task.CompletedTask;
    }

    private static Order Copy(Order o) => new()
    {
        Id=o.Id, Email=o.Email, Amount=o.Amount, Currency=o.Currency, Provider=o.Provider,
        Status=o.Status, TransactionId=o.TransactionId, UpdatedAtUtc=o.UpdatedAtUtc
    };
}
