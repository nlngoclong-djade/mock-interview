using LegacyStore.Models;

namespace LegacyStore.Helpers;

public class CheckValidProduct
{
    public CheckValidProduct()
    {
        
    }

    public CheckoutResult? CheckSKU(string sku)
    {
        return string.IsNullOrEmpty(sku) ? null : new CheckoutResult(false, "SKU is required", 0, null);
    }
}