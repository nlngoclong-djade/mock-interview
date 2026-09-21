using LegacyPayments;

var tests = new (string Name, Action Run)[]
{
    ("creates USD payment and sends receipt", CreatesUsdPayment),
    ("applies priority fee discount", AppliesPriorityDiscount),
    ("routes large payment to review", RoutesLargePaymentToReview),
    ("rejects duplicate payment id", RejectsDuplicateId),
    ("fully refunds a paid payment", FullyRefundsPayment),
    ("rejects refund over original amount", RejectsOversizedRefund),
    ("cancels payment under review", CancelsReviewedPayment),
    ("returns detached stored objects", ReturnsDetachedObjects),
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

static (LegacyPaymentProcessor Processor, InMemoryPaymentDataStore Store, FakeSender Sender) CreateSystem()
{
    var store = new InMemoryPaymentDataStore();
    var sender = new FakeSender();
    var processor = new LegacyPaymentProcessor(new PaymentRepository(store), sender, new FixedClock());
    return (processor, store, sender);
}

static void CreatesUsdPayment()
{
    var (processor, _, sender) = CreateSystem();
    var result = processor.MakePayment(" p-1 ", "buyer@example.com", 100m, "usd", false);
    Equal(true, result.Success); Equal("PAID", result.Payment!.Status); Equal(3.20m, result.Payment.Fee);
    Equal("p-1", result.Payment.Id); Equal(1, sender.Messages.Count); Equal("Payment received", sender.Messages[0].Subject);
}

static void AppliesPriorityDiscount()
{
    var (processor, _, _) = CreateSystem();
    var result = processor.MakePayment("p-2", "buyer@example.com", 100m, "EUR", true);
    Equal(2.20m, result.Payment!.Fee);
}

static void RoutesLargePaymentToReview()
{
    var (processor, _, sender) = CreateSystem();
    var result = processor.MakePayment("p-3", "buyer@example.com", 5000m, "GBP", false);
    Equal("REVIEW", result.Payment!.Status); Equal("Payment requires review", result.Message);
    Equal("Payment under review", sender.Messages[0].Subject);
}

static void RejectsDuplicateId()
{
    var (processor, store, sender) = CreateSystem();
    processor.MakePayment("same", "a@example.com", 10m, "USD", false);
    var result = processor.MakePayment("same", "b@example.com", 20m, "USD", false);
    Equal(false, result.Success); Equal("Payment already exists", result.Message); Equal(1, store.SaveCount); Equal(1, sender.Messages.Count);
}

static void FullyRefundsPayment()
{
    var (processor, _, sender) = CreateSystem();
    processor.MakePayment("p-4", "buyer@example.com", 50m, "USD", false);
    var result = processor.Refund("p-4", 50m);
    Equal(true, result.Success); Equal("REFUNDED", result.Payment!.Status); Equal(2, sender.Messages.Count);
}

static void RejectsOversizedRefund()
{
    var (processor, store, sender) = CreateSystem();
    processor.MakePayment("p-5", "buyer@example.com", 50m, "USD", false);
    var result = processor.Refund("p-5", 50.01m);
    Equal(false, result.Success); Equal("Refund exceeds payment amount", result.Message); Equal(1, store.SaveCount); Equal(1, sender.Messages.Count);
}

static void CancelsReviewedPayment()
{
    var (processor, _, _) = CreateSystem();
    processor.MakePayment("p-6", "buyer@example.com", 6000m, "EUR", false);
    var result = processor.Cancel("p-6");
    Equal(true, result.Success); Equal("CANCELLED", result.Payment!.Status);
}

static void ReturnsDetachedObjects()
{
    var (processor, _, _) = CreateSystem();
    processor.MakePayment("p-7", "buyer@example.com", 25m, "USD", false);
    var first = processor.Find("p-7")!;
    first.Status = "CORRUPTED";
    Equal("PAID", processor.Find("p-7")!.Status);
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

sealed class FakeSender : IReceiptSender
{
    public List<(string Email, string Subject, string Body)> Messages { get; } = new();
    public void Send(string email, string subject, string body) => Messages.Add((email, subject, body));
}
