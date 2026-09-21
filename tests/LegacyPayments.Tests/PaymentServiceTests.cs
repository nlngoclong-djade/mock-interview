using Xunit;

namespace LegacyPayments.Tests;

public sealed class PaymentServiceTests
{
    [Fact]
    public void Authorize_ValidUsdPayment_PersistsAndNotifies()
    {
        var (service, store, notifier) = CreateSystem();

        var result = service.Authorize(" pay-1 ", "buyer@example.com", 100m, "usd", false);

        Assert.True(result.Success);
        Assert.Equal("pay-1", result.Payment!.Id);
        Assert.Equal("AUTHORIZED", result.Payment.Status);
        Assert.Equal(3.05m, result.Payment.ProcessingFee);
        Assert.Single(result.Payment.AuditTrail);
        Assert.Equal(1, store.SaveCount);
        Assert.Single(notifier.Messages);
    }

    [Fact]
    public void Authorize_PreferredCustomer_AppliesFeeDiscount()
    {
        var (service, _, _) = CreateSystem();

        var result = service.Authorize("pay-2", "buyer@example.com", 100m, "EUR", true);

        Assert.Equal(1.95m, result.Payment!.ProcessingFee);
    }

    [Fact]
    public void Authorize_LargePayment_RequiresReview()
    {
        var (service, _, notifier) = CreateSystem();

        var result = service.Authorize("pay-3", "buyer@example.com", 3000m, "USD", false);

        Assert.Equal("REVIEW", result.Payment!.Status);
        Assert.Equal("Authorization requires review", result.Message);
        Assert.Equal("Payment under review", notifier.Messages[0].Subject);
    }

    [Fact]
    public void Capture_AuthorizedPayment_ChangesStatusAndAddsAuditEntry()
    {
        var (service, _, _) = CreateSystem();
        service.Authorize("pay-4", "buyer@example.com", 50m, "USD", false);

        var result = service.Capture("pay-4");

        Assert.True(result.Success);
        Assert.Equal("CAPTURED", result.Payment!.Status);
        Assert.Equal(2, result.Payment.AuditTrail.Count);
    }

    [Fact]
    public void Void_ReviewPayment_ChangesStatus()
    {
        var (service, _, _) = CreateSystem();
        service.Authorize("pay-5", "buyer@example.com", 4000m, "USD", false);

        var result = service.Void("pay-5");

        Assert.True(result.Success);
        Assert.Equal("VOIDED", result.Payment!.Status);
    }

    [Fact]
    public void Find_DifferentIdCasing_FindsSamePayment()
    {
        var (service, _, _) = CreateSystem();
        service.Authorize("Payment-AbC", "buyer@example.com", 20m, "USD", false);

        Assert.NotNull(service.Find("payment-abc"));
        Assert.NotNull(service.Find(" PAYMENT-ABC "));
    }

    [Fact]
    public void Find_MutatedPayment_DoesNotChangePersistedPayment()
    {
        var (service, _, _) = CreateSystem();
        service.Authorize("pay-6", "buyer@example.com", 20m, "USD", false);
        var first = service.Find("pay-6")!;

        first.Status = "CORRUPTED";
        first.AuditTrail.Add("CORRUPTED");

        var second = service.Find("pay-6")!;
        Assert.Equal("AUTHORIZED", second.Status);
        Assert.Single(second.AuditTrail);
    }

    [Fact]
    public void Capture_PersistenceFailure_DoesNotPersistChangedStatus()
    {
        var (service, store, _) = CreateSystem();
        service.Authorize("pay-7", "buyer@example.com", 20m, "USD", false);
        store.FailNextSave = true;

        Assert.Throws<InvalidOperationException>(() => service.Capture("pay-7"));

        Assert.Equal("AUTHORIZED", service.Find("pay-7")!.Status);
    }

    [Fact]
    public void LruCache_OverCapacity_EvictsLeastRecentlyUsedItem()
    {
        var cache = new LruCache<string, int>(2);
        cache.Put("a", 1);
        cache.Put("b", 2);
        Assert.True(cache.TryGet("a", out _));

        cache.Put("c", 3);

        Assert.False(cache.TryGet("b", out _));
        Assert.True(cache.TryGet("a", out var a));
        Assert.Equal(1, a);
        Assert.True(cache.TryGet("c", out var c));
        Assert.Equal(3, c);
    }

    private static (LegacyPaymentService Service, InMemoryPaymentStore Store, FakeNotifier Notifier) CreateSystem()
    {
        var store = new InMemoryPaymentStore();
        var notifier = new FakeNotifier();
        var service = new LegacyPaymentService(new PaymentRepository(store), notifier, new FixedClock());
        return (service, store, notifier);
    }

    private sealed class FixedClock : IClock
    {
        public DateTime UtcNow => new(2026, 9, 21, 0, 0, 0, DateTimeKind.Utc);
    }

    private sealed class FakeNotifier : IPaymentNotifier
    {
        public List<(string Email, string Subject, string Body)> Messages { get; } = new();

        public void Send(string email, string subject, string body)
        {
            Messages.Add((email, subject, body));
        }
    }
}
