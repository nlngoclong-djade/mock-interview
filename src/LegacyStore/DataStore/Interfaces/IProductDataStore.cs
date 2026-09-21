using LegacyStore.Models;

namespace LegacyStore.DataStore.Interfaces;

public interface IProductDataStore
{
    Product? Load(string sku);
    void Save(Product product);
}