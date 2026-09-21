using LegacyStore.DataStore.Interfaces;
using LegacyStore.Helpers;
using LegacyStore.Models;
using LegacyStore.Repositories;

namespace LegacyStore.Services;

public sealed class LegacyCatalogService
{
    private readonly ProductRepository repository;
    private readonly IStoreNotifier notifier;
    private readonly IClock clock;
    private readonly CheckValidProduct _checkValidProduct = new CheckValidProduct();

    public LegacyCatalogService(ProductRepository repository, IStoreNotifier notifier, IClock clock)
    {
        this.repository = repository;
        this.notifier = notifier;
        this.clock = clock;
    }

    public CheckoutResult Checkout(string sku, int quantity, string customerEmail, bool premiumCustomer)
    {
        _checkValidProduct.CheckSKU(sku);
        if (quantity <= 0) return new CheckoutResult(false, "Quantity must be positive", 0, null);
        if (string.IsNullOrEmpty(customerEmail) || !customerEmail.Contains('@')) return new CheckoutResult(false, "A valid email is required", 0, null);

        var product = repository.Get(sku.Trim());
        if (product == null) return new CheckoutResult(false, "Product not found", 0, null);
        if (product.Status != nameof(ProductStatus.ACTIVE)) return new CheckoutResult(false, "Product is not available", 0, product);
        if (product.Stock < quantity) return new CheckoutResult(false, "Insufficient stock", 0, product);

        var total = product.UnitPrice * quantity;
        if (premiumCustomer && total >= 100) total = total * 0.90m;
        else if (total >= 500) total = total * 0.95m;
        total = Math.Round(total, 2, MidpointRounding.AwayFromZero);

        product.Stock = product.Stock - quantity;
        if (product.Stock == 0) product.Status = nameof(ProductStatus.OUT_OF_STOCK);
        product.UpdatedAtUtc = clock.UtcNow;
        repository.Save(product);

        notifier.Send(customerEmail.Trim(), "Order confirmed", "Purchased " + quantity + " x " + product.Name + " (" + product.Sku + "). Total: " + total.ToString("0.00"));
        if (product.Stock > 0 && product.Stock <= 3)
            notifier.Send("inventory@example.com", "Low stock", product.Sku + " has " + product.Stock + " units remaining.");

        return new CheckoutResult(true, "Checkout completed", total, product);
    }

    public CheckoutResult Restock(string sku, int quantity)
    {
        if (string.IsNullOrEmpty(sku)) return new CheckoutResult(false, "SKU is required", 0, null);
        if (quantity <= 0) return new CheckoutResult(false, "Quantity must be positive", 0, null);
        var product = repository.Get(sku.Trim());
        if (product == null) return new CheckoutResult(false, "Product not found", 0, null);
        product.Stock = product.Stock + quantity;
        if (product.Status == nameof(ProductStatus.OUT_OF_STOCK)) product.Status = nameof(ProductStatus.ACTIVE);
        product.UpdatedAtUtc = clock.UtcNow;
        repository.Save(product);
        return new CheckoutResult(true, "Restock completed", 0, product);
    }

    public CheckoutResult ChangePrice(string sku, decimal newPrice)
    {
        if (string.IsNullOrEmpty(sku)) return new CheckoutResult(false, "SKU is required", 0, null);
        if (newPrice <= 0) return new CheckoutResult(false, "Price must be positive", 0, null);
        var product = repository.Get(sku.Trim());
        if (product == null) return new CheckoutResult(false, "Product not found", 0, null);
        product.UnitPrice = newPrice;
        product.UpdatedAtUtc = clock.UtcNow;
        repository.Save(product);
        return new CheckoutResult(true, "Price changed", 0, product);
    }

    public Product? Find(string sku)
    {
        return string.IsNullOrEmpty(sku) ? null : repository.Get(sku.Trim());
    }
}
