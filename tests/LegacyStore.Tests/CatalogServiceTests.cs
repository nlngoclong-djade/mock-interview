using LegacyStore.DataStore.Implementation;
using LegacyStore.DataStore.Interfaces;
using LegacyStore.Helpers;
using LegacyStore.Models;
using LegacyStore.Repositories;
using LegacyStore.Services;
using Xunit;

namespace LegacyStore.Tests;

public sealed class CatalogServiceTests
{
    [Fact]
    public void Checkout_ActiveProduct_CompletesOrder()
    {
        var (service, _, notifier) = CreateSystem();

        var result = service.Checkout("sku-1", 2, "buyer@example.com", false);

        Assert.True(result.Success);
        Assert.Equal(50m, result.Total);
        Assert.Equal(8, result.Product!.Stock);
        Assert.Single(notifier.Messages);
    }

    [Fact]
    public void Checkout_PremiumCustomer_AppliesPremiumDiscount()
    {
        var (service, _, _) = CreateSystem(price: 60m);

        var result = service.Checkout("sku-1", 2, "buyer@example.com", true);

        Assert.Equal(108m, result.Total);
    }

    [Fact]
    public void Checkout_QualifyingOrder_AppliesBulkDiscount()
    {
        var (service, _, _) = CreateSystem(stock: 20, price: 100m);

        var result = service.Checkout("sku-1", 5, "buyer@example.com", false);

        Assert.Equal(475m, result.Total);
    }

    [Fact]
    public void Checkout_InsufficientStock_DoesNotSaveOrNotify()
    {
        var (service, store, notifier) = CreateSystem(stock: 1);

        var result = service.Checkout("sku-1", 2, "buyer@example.com", false);

        Assert.False(result.Success);
        Assert.Equal("Insufficient stock", result.Message);
        Assert.Equal(1, store.SaveCount);
        Assert.Empty(notifier.Messages);
    }

    [Fact]
    public void Checkout_AllRemainingStock_MarksProductOutOfStock()
    {
        var (service, _, _) = CreateSystem(stock: 2);

        var result = service.Checkout("sku-1", 2, "buyer@example.com", false);

        Assert.Equal("OUT_OF_STOCK", result.Product!.Status);
        Assert.Equal(0, result.Product.Stock);
    }

    [Fact]
    public void Checkout_LowRemainingStock_SendsInventoryAlert()
    {
        var (service, _, notifier) = CreateSystem(stock: 5);

        service.Checkout("sku-1", 2, "buyer@example.com", false);

        Assert.Equal(2, notifier.Messages.Count);
        Assert.Equal("inventory@example.com", notifier.Messages[1].Recipient);
    }

    [Fact]
    public void Restock_DepletedProduct_ReactivatesProduct()
    {
        var (service, _, _) = CreateSystem(stock: 1);
        service.Checkout("sku-1", 1, "buyer@example.com", false);

        var result = service.Restock("sku-1", 4);

        Assert.True(result.Success);
        Assert.Equal("ACTIVE", result.Product!.Status);
        Assert.Equal(4, result.Product.Stock);
    }

    [Fact]
    public void Find_MutatedReturnValue_DoesNotChangeStoredProduct()
    {
        var (service, _, _) = CreateSystem();
        var first = service.Find("sku-1")!;

        first.Name = "CORRUPTED";

        Assert.Equal("Headphones", service.Find("sku-1")!.Name);
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

    private static (LegacyCatalogService Service, InMemoryProductDataStore Store, FakeNotifier Notifier) CreateSystem(
        int stock = 10,
        decimal price = 25m)
    {
        var store = new InMemoryProductDataStore();
        store.Save(new Product()
        {
            Sku = "sku-1",
            Name = "Headphones",
            UnitPrice = price,
            Stock = stock,
            Status = "ACTIVE"
        });
        var notifier = new FakeNotifier();
        var service = new LegacyCatalogService(new ProductRepository(store), notifier, new FixedClock());
        return (service, store, notifier);
    }

    private sealed class FixedClock : IClock
    {
        public DateTime UtcNow => new(2026, 9, 21, 0, 0, 0, DateTimeKind.Utc);
    }

    private sealed class FakeNotifier : IStoreNotifier
    {
        public List<(string Recipient, string Subject, string Body)> Messages { get; } = new();

        public void Send(string recipient, string subject, string body)
        {
            Messages.Add((recipient, subject, body));
        }
    }
}
