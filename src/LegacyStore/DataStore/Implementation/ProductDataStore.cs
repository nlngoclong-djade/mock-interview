
using LegacyStore.DataStore.Interfaces;
using LegacyStore.Models;

namespace LegacyStore.DataStore.Implementation;

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
