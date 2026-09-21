namespace LegacyStore;

public interface IProductDataStore
{
    Product? Load(string sku);
    void Save(Product product);
}

public sealed class InMemoryProductDataStore : IProductDataStore
{
    private readonly Dictionary<string, Product> _products = new(StringComparer.OrdinalIgnoreCase);

    public int LoadCount { get; private set; }
    public int SaveCount { get; private set; }

    public Product? Load(string sku)
    {
        LoadCount++;
        return _products.TryGetValue(sku, out var product) ? Copy(product) : null;
    }

    public void Save(Product product)
    {
        SaveCount++;
        _products[product.Sku] = Copy(product);
    }

    private static Product Copy(Product product) => new()
    {
        Sku = product.Sku,
        Name = product.Name,
        UnitPrice = product.UnitPrice,
        Stock = product.Stock,
        Status = product.Status,
        UpdatedAtUtc = product.UpdatedAtUtc
    };
}

public class ProductRepository
{
    private readonly IProductDataStore _store;

    public ProductRepository(IProductDataStore store)
    {
        _store = store;
    }

    public Product? Get(string sku)
    {
        return _store.Load(sku);
    }

    public void Save(Product product)
    {
        _store.Save(product);
    }
}
