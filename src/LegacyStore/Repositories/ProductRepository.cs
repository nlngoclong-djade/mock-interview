using LegacyStore.DataStore.Interfaces;
using LegacyStore.Models;

namespace LegacyStore.Repositories;

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