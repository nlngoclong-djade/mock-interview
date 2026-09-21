namespace OrderApplication;

public sealed class LegacyCheckoutService
{
    private readonly IOrderStore _store;
    private readonly IStripeClient _stripe;
    private readonly IPayPalClient _paypal;
    private readonly IEmailSender _email;
    private readonly IClock _clock;

    public LegacyCheckoutService(IOrderStore store, IStripeClient stripe, IPayPalClient paypal, IEmailSender email, IClock clock)
    {
        _store = store; _stripe = stripe; _paypal = paypal; _email = email; _clock = clock;
    }

    public async Task<CheckoutResult> CheckoutAsync(string id, string email, decimal amount, string currency, string provider, CancellationToken cancellationToken = default)
    {
        if (id == null || id.Trim() == "") return new(false, "Order id is required", null);
        if (email == null || email.Trim() == "" || !email.Contains('@')) return new(false, "A valid email is required", null);
        if (amount <= 0) return new(false, "Amount must be positive", null);
        var c = currency == null ? "" : currency.Trim().ToUpperInvariant();
        if (c != "USD" && c != "EUR") return new(false, "Unsupported currency", null);
        var p = provider == null ? "" : provider.Trim().ToUpperInvariant();
        if (p != "STRIPE" && p != "PAYPAL") return new(false, "Unsupported provider", null);

        var orderId = id.Trim();
        var existing = await _store.LoadAsync(orderId, cancellationToken);
        if (existing?.Status == "COMPLETED") return new(true, "Order already completed", existing);

        string transaction;
        if (p == "STRIPE")
            transaction = await _stripe.CreateChargeAsync(orderId, amount, c, cancellationToken);
        else
            transaction = await _paypal.PayAsync(orderId, amount, c, cancellationToken);

        var order = existing ?? new Order();
        order.Id = orderId; order.Email = email.Trim(); order.Amount = amount; order.Currency = c;
        order.Provider = p; order.Status = "COMPLETED"; order.TransactionId = transaction; order.UpdatedAtUtc = _clock.UtcNow;
        await _store.SaveAsync(order, cancellationToken);

        if (p == "STRIPE")
            await _email.SendAsync(order.Email, "Order completed", "Stripe payment " + transaction + " completed for " + amount.ToString("0.00") + " " + c, cancellationToken);
        else
            await _email.SendAsync(order.Email, "Order completed", "PayPal payment " + transaction + " completed for " + amount.ToString("0.00") + " " + c, cancellationToken);

        return new(true, "Checkout completed", order);
    }

    public Task<CheckoutResult> CheckoutAsync(string id, string email, decimal amount, string currency, string provider, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        // Task 2
        return CheckoutAsync(id, email, amount, currency, provider, cancellationToken);
    }

    public Task<Order?> FindAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return Task.FromResult<Order?>(null);
        return _store.LoadAsync(id.Trim(), cancellationToken);
    }
}
