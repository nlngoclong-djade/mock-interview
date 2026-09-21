using LegacyStore;

var tests = new (string Name, Action Run)[]
{
    ("checks out active product", ChecksOutProduct),
    ("applies premium discount", AppliesPremiumDiscount),
    ("applies bulk discount", AppliesBulkDiscount),
    ("rejects insufficient stock", RejectsInsufficientStock),
    ("marks depleted product out of stock", MarksProductOutOfStock),
    ("sends low-stock alert", SendsLowStockAlert),
    ("restock reactivates depleted product", RestockReactivatesProduct),
    ("returns detached stored products", ReturnsDetachedProducts),
    ("LRU cache evicts least recently used item", LruEvictionWorks)
};

var failures = 0;
foreach (var test in tests)
{
    try { test.Run(); Console.WriteLine($"PASS  {test.Name}"); }
    catch (Exception ex) { failures++; Console.WriteLine($"FAIL  {test.Name}: {ex.Message}"); }
}

Console.WriteLine($"\n{tests.Length - failures}/{tests.Length} checks passed");
return failures == 0 ? 0 : 1;

static (LegacyCatalogService Service, InMemoryProductDataStore Store, FakeNotifier Notifier) CreateSystem(int stock = 10, decimal price = 25m)
{
    var store = new InMemoryProductDataStore();
    store.Save(new Product { Sku = "sku-1", Name = "Headphones", UnitPrice = price, Stock = stock, Status = "ACTIVE" });
    var notifier = new FakeNotifier();
    var service = new LegacyCatalogService(new ProductRepository(store), notifier, new FixedClock());
    return (service, store, notifier);
}

static void ChecksOutProduct()
{
    var (service, _, notifier) = CreateSystem();
    var result = service.Checkout("sku-1", 2, "buyer@example.com", false);
    Equal(true, result.Success); Equal(50m, result.Total); Equal(8, result.Product!.Stock); Equal(1, notifier.Messages.Count);
}

static void AppliesPremiumDiscount()
{
    var (service, _, _) = CreateSystem(price: 60m);
    Equal(108m, service.Checkout("sku-1", 2, "buyer@example.com", true).Total);
}

static void AppliesBulkDiscount()
{
    var (service, _, _) = CreateSystem(stock: 20, price: 100m);
    Equal(475m, service.Checkout("sku-1", 5, "buyer@example.com", false).Total);
}

static void RejectsInsufficientStock()
{
    var (service, store, notifier) = CreateSystem(stock: 1);
    var result = service.Checkout("sku-1", 2, "buyer@example.com", false);
    Equal(false, result.Success); Equal("Insufficient stock", result.Message); Equal(1, store.SaveCount); Equal(0, notifier.Messages.Count);
}

static void MarksProductOutOfStock()
{
    var (service, _, _) = CreateSystem(stock: 2);
    var result = service.Checkout("sku-1", 2, "buyer@example.com", false);
    Equal("OUT_OF_STOCK", result.Product!.Status); Equal(0, result.Product.Stock);
}

static void SendsLowStockAlert()
{
    var (service, _, notifier) = CreateSystem(stock: 5);
    service.Checkout("sku-1", 2, "buyer@example.com", false);
    Equal(2, notifier.Messages.Count); Equal("inventory@example.com", notifier.Messages[1].Recipient);
}

static void RestockReactivatesProduct()
{
    var (service, _, _) = CreateSystem(stock: 1);
    service.Checkout("sku-1", 1, "buyer@example.com", false);
    var result = service.Restock("sku-1", 4);
    Equal(true, result.Success); Equal("ACTIVE", result.Product!.Status); Equal(4, result.Product.Stock);
}

static void ReturnsDetachedProducts()
{
    var (service, _, _) = CreateSystem();
    var first = service.Find("sku-1")!;
    first.Name = "CORRUPTED";
    Equal("Headphones", service.Find("sku-1")!.Name);
}

static void LruEvictionWorks()
{
    var cache = new LruCache<string, int>(2);
    cache.Put("a", 1); cache.Put("b", 2); True(cache.TryGet("a", out _)); cache.Put("c", 3);
    Equal(false, cache.TryGet("b", out _)); True(cache.TryGet("a", out var a)); Equal(1, a); True(cache.TryGet("c", out var c)); Equal(3, c);
}

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new Exception($"expected <{expected}> but was <{actual}>");
}

static void True(bool value)
{
    if (!value) throw new Exception("expected true but was false");
}

sealed class FixedClock : IClock
{
    public DateTime UtcNow => new(2026, 9, 21, 0, 0, 0, DateTimeKind.Utc);
}

sealed class FakeNotifier : IStoreNotifier
{
    public List<(string Recipient, string Subject, string Body)> Messages { get; } = new();
    public void Send(string recipient, string subject, string body) => Messages.Add((recipient, subject, body));
}
