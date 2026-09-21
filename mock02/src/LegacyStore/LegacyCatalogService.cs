namespace LegacyStore;

public interface IStoreNotifier
{
    void Send(string recipient, string subject, string body);
}

public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public sealed class LegacyCatalogService
{
    private ProductRepository repository;
    private IStoreNotifier notifier;
    private IClock clock;

    public LegacyCatalogService(ProductRepository repository, IStoreNotifier notifier, IClock clock)
    {
        this.repository = repository;
        this.notifier = notifier;
        this.clock = clock;
    }

    public CheckoutResult Checkout(string sku, int quantity, string customerEmail, bool premiumCustomer)
    {
        if (sku == null || sku.Trim() == "") return new CheckoutResult(false, "SKU is required", 0, null);
        if (quantity <= 0) return new CheckoutResult(false, "Quantity must be positive", 0, null);
        if (customerEmail == null || customerEmail.Trim() == "" || !customerEmail.Contains('@')) return new CheckoutResult(false, "A valid email is required", 0, null);

        var p = repository.Get(sku.Trim());
        if (p == null) return new CheckoutResult(false, "Product not found", 0, null);
        if (p.Status != "ACTIVE") return new CheckoutResult(false, "Product is not available", 0, p);
        if (p.Stock < quantity) return new CheckoutResult(false, "Insufficient stock", 0, p);

        var total = p.UnitPrice * quantity;
        if (premiumCustomer && total >= 100) total = total * 0.90m;
        else if (total >= 500) total = total * 0.95m;
        total = Math.Round(total, 2, MidpointRounding.AwayFromZero);

        p.Stock = p.Stock - quantity;
        if (p.Stock == 0) p.Status = "OUT_OF_STOCK";
        p.UpdatedAtUtc = clock.UtcNow;
        repository.Save(p);

        notifier.Send(customerEmail.Trim(), "Order confirmed", "Purchased " + quantity + " x " + p.Name + " (" + p.Sku + "). Total: " + total.ToString("0.00"));
        if (p.Stock > 0 && p.Stock <= 3)
            notifier.Send("inventory@example.com", "Low stock", p.Sku + " has " + p.Stock + " units remaining.");

        return new CheckoutResult(true, "Checkout completed", total, p);
    }

    public CheckoutResult Restock(string sku, int quantity)
    {
        if (sku == null || sku.Trim() == "") return new CheckoutResult(false, "SKU is required", 0, null);
        if (quantity <= 0) return new CheckoutResult(false, "Quantity must be positive", 0, null);
        var p = repository.Get(sku.Trim());
        if (p == null) return new CheckoutResult(false, "Product not found", 0, null);
        p.Stock = p.Stock + quantity;
        if (p.Status == "OUT_OF_STOCK") p.Status = "ACTIVE";
        p.UpdatedAtUtc = clock.UtcNow;
        repository.Save(p);
        return new CheckoutResult(true, "Restock completed", 0, p);
    }

    public CheckoutResult ChangePrice(string sku, decimal newPrice)
    {
        if (sku == null || sku.Trim() == "") return new CheckoutResult(false, "SKU is required", 0, null);
        if (newPrice <= 0) return new CheckoutResult(false, "Price must be positive", 0, null);
        var p = repository.Get(sku.Trim());
        if (p == null) return new CheckoutResult(false, "Product not found", 0, null);
        p.UnitPrice = newPrice;
        p.UpdatedAtUtc = clock.UtcNow;
        repository.Save(p);
        return new CheckoutResult(true, "Price changed", 0, p);
    }

    public Product? Find(string sku)
    {
        if (sku == null || sku.Trim() == "") return null;
        return repository.Get(sku.Trim());
    }
}
