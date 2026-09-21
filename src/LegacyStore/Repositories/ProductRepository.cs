using LegacyStore.DataStore.Interfaces;
using LegacyStore.Helpers;
using LegacyStore.Models;

namespace LegacyStore.Repositories;

public class ProductRepository
{
    int capacity = 10; // get from config but not know how to get it here
    private readonly IProductDataStore _store;
    private readonly LruCache<string, Product> _lruCache = new LruCache<string, Product>(10);

    public ProductRepository(IProductDataStore store)
    {
        _store = store;
    }

    public Product? Get(string sku)
    {
        /// cached
        if (_lruCache.TryGet(sku, out var product))
        {
            return product;
        }
        /// not cached
        var cacheValue =  _store.Load(sku);
        _lruCache.Put(sku, cacheValue);
        return cacheValue;
    }

    public void Save(Product product)
    {
        _lruCache.Remove(product.Sku);
        _store.Save(product);
        _lruCache.Put(product.Sku, product);
    }
}