namespace ConcurrentOrders;

public interface IPaymentGateway
{
    Task<string> ChargeAsync(string orderId, decimal amount, CancellationToken cancellationToken = default);
}

public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public sealed class OrderProcessor
{
    private readonly IOrderStore _store;
    private readonly IPaymentGateway _gateway;
    private readonly IClock _clock;

    public OrderProcessor(IOrderStore store, IPaymentGateway gateway, IClock clock)
    {
        _store = store;
        _gateway = gateway;
        _clock = clock;
    }

    public async Task<ProcessResult> ProcessAsync(
        string orderId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            return new ProcessResult(false, "Order id is required", null);

        if (amount <= 0)
            return new ProcessResult(false, "Amount must be positive", null);

        var id = orderId.Trim();
        var existing = await _store.LoadAsync(id, cancellationToken);

        if (existing?.Status == "COMPLETED")
            return new ProcessResult(true, "Order already completed", existing);

        var order = existing ?? new Order { Id = id, Amount = amount };

        var transactionId = await _gateway.ChargeAsync(id, amount, cancellationToken);

        order.Amount = amount;
        order.TransactionId = transactionId;
        order.Status = "COMPLETED";
        order.UpdatedAtUtc = _clock.UtcNow;

        await _store.SaveAsync(order, cancellationToken);
        return new ProcessResult(true, "Order completed", order);
    }

    public Task<ProcessResult> ProcessAsync(
        string orderId,
        decimal amount,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        // Task 2: implement idempotent request handling without breaking
        // the existing ProcessAsync overload.
        return ProcessAsync(orderId, amount, cancellationToken);
    }

    public Task<Order?> FindAsync(string orderId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            return Task.FromResult<Order?>(null);

        return _store.LoadAsync(orderId.Trim(), cancellationToken);
    }
}
