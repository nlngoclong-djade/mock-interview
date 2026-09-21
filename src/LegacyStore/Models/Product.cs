namespace LegacyStore.Models;

public sealed class Product
{
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Stock { get; set; }
    public string Status { get; set; } = "";
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed record CheckoutResult(bool Success, string Message, decimal Total, Product? Product);
