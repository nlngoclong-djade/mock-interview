using ConcurrentOrders;

var tests = new (string Name, Func<Task> Run)[]
{
    ("processes pending order", ProcessesPendingOrder),
    ("completed order is not charged again", CompletedOrderIsNotChargedAgain),
    ("missing order returns failure", MissingOrderReturnsFailure),
    ("trims order id", TrimsOrderId),
    ("idempotency overload preserves normal success", IdempotencyOverloadWorks)
};

var failures = 0;
foreach (var test in tests)
{
    try
    {
        await test.Run();
        Console.WriteLine($"PASS  {test.Name}");
    }
    catch (Exception ex)
    {
        failures++;
        Console.WriteLine($"FAIL  {test.Name}: {ex.Message}");
    }
}

Console.WriteLine($"\n{tests.Length - failures}/{tests.Length} checks passed");
return failures == 0 ? 0 : 1;

static (OrderService Service, InMemoryOrderStore Store, FakeGateway Gateway) CreateSystem()
{
    var store = new InMemoryOrderStore();
    var gateway = new FakeGateway();
    return (new OrderService(store, gateway), store, gateway);
}

static async Task ProcessesPendingOrder()
{
    var (service, store, gateway) = CreateSystem();
    store.Seed(new Order { Id = "o-1", Amount = 25m });
    var result = await service.ProcessAsync("o-1");
    Equal(true, result.Success);
    Equal("COMPLETED", result.Order!.Status);
    Equal(1, gateway.ChargeCount);
}

static async Task CompletedOrderIsNotChargedAgain()
{
    var (service, store, gateway) = CreateSystem();
    store.Seed(new Order { Id = "o-2", Amount = 30m, Status = "COMPLETED", ChargeId = "existing" });
    var result = await service.ProcessAsync("o-2");
    Equal(true, result.Success);
    Equal("Order already completed", result.Message);
    Equal(0, gateway.ChargeCount);
}

static async Task MissingOrderReturnsFailure()
{
    var (service, _, gateway) = CreateSystem();
    var result = await service.ProcessAsync("missing");
    Equal(false, result.Success);
    Equal("Order not found", result.Message);
    Equal(0, gateway.ChargeCount);
}

static async Task TrimsOrderId()
{
    var (service, store, _) = CreateSystem();
    store.Seed(new Order { Id = "o-3", Amount = 10m });
    var result = await service.ProcessAsync("  o-3  ");
    Equal(true, result.Success);
}

static async Task IdempotencyOverloadWorks()
{
    var (service, store, gateway) = CreateSystem();
    store.Seed(new Order { Id = "o-4", Amount = 12m });
    var result = await service.ProcessAsync("o-4", "request-1");
    Equal(true, result.Success);
    Equal(1, gateway.ChargeCount);
}

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new Exception($"expected <{expected}> but was <{actual}>");
}

sealed class FakeGateway : IPaymentGateway
{
    private int _chargeCount;
    public int ChargeCount => _chargeCount;

    public async Task<string> ChargeAsync(decimal amount, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _chargeCount);
        await Task.Delay(10, cancellationToken);
        return $"ch-{ChargeCount}";
    }
}
