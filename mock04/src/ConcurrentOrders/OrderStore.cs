namespace ConcurrentOrders;

public interface IOrderStore
{
    Task<Order?> LoadAsync(string id, CancellationToken cancellationToken = default);
    Task SaveAsync(Order order, CancellationToken cancellationToken = default);
}

public sealed class InMemoryOrderStore : IOrderStore
{
    private readonly Dictionary<string, Order> _orders = new(StringComparer.OrdinalIgnoreCase);

    public Task<Order?> LoadAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_orders)
            return Task.FromResult(_orders.TryGetValue(id, out var order) ? Copy(order) : null);
    }

    public Task SaveAsync(Order order, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_orders)
            _orders[order.Id] = Copy(order);
        return Task.CompletedTask;
    }

    public void Seed(Order order)
    {
        lock (_orders)
            _orders[order.Id] = Copy(order);
    }

    private static Order Copy(Order order) => new()
    {
        Id = order.Id,
        Amount = order.Amount,
        Status = order.Status,
        ChargeId = order.ChargeId
    };
}
