namespace LegacyPayments;

public interface IPaymentNotifier
{
    void Send(string email, string subject, string body);
}

public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public sealed class LegacyPaymentService
{
    private PaymentRepository repo;
    private IPaymentNotifier notifier;
    private IClock clock;

    public LegacyPaymentService(PaymentRepository repo, IPaymentNotifier notifier, IClock clock)
    {
        this.repo = repo;
        this.notifier = notifier;
        this.clock = clock;
    }

    public PaymentResult Authorize(string id, string email, decimal amount, string currency, bool preferredCustomer)
    {
        if (id == null || id.Trim() == "") return new PaymentResult(false, "Payment id is required", null);
        if (email == null || email.Trim() == "" || !email.Contains('@')) return new PaymentResult(false, "A valid email is required", null);
        if (amount <= 0) return new PaymentResult(false, "Amount must be positive", null);
        var normalizedCurrency = currency == null ? "" : currency.Trim().ToUpperInvariant();
        if (normalizedCurrency != "USD" && normalizedCurrency != "EUR") return new PaymentResult(false, "Unsupported currency", null);
        if (repo.Get(id.Trim()) != null) return new PaymentResult(false, "Payment already exists", null);

        decimal fee;
        if (normalizedCurrency == "USD") fee = amount * 0.028m + 0.25m;
        else fee = amount * 0.024m + 0.20m;
        if (preferredCustomer) fee = fee * 0.75m;
        fee = Math.Round(fee, 2, MidpointRounding.AwayFromZero);

        var payment = new Payment();
        payment.Id = id.Trim();
        payment.CustomerEmail = email.Trim();
        payment.Amount = amount;
        payment.Currency = normalizedCurrency;
        payment.Status = amount >= 3000 ? "REVIEW" : "AUTHORIZED";
        payment.ProcessingFee = fee;
        payment.UpdatedAtUtc = clock.UtcNow;
        payment.AuditTrail.Add("Authorized at " + payment.UpdatedAtUtc.ToString("O"));
        repo.Save(payment);

        if (payment.Status == "AUTHORIZED")
            notifier.Send(payment.CustomerEmail, "Payment authorized", "Payment " + payment.Id + " was authorized.");
        else
            notifier.Send(payment.CustomerEmail, "Payment under review", "Payment " + payment.Id + " requires review.");

        return new PaymentResult(true, payment.Status == "AUTHORIZED" ? "Authorization completed" : "Authorization requires review", payment);
    }

    public PaymentResult Capture(string id)
    {
        if (id == null || id.Trim() == "") return new PaymentResult(false, "Payment id is required", null);
        var payment = repo.Get(id.Trim());
        if (payment == null) return new PaymentResult(false, "Payment not found", null);
        if (payment.Status != "AUTHORIZED") return new PaymentResult(false, "Only authorized payments can be captured", payment);
        payment.Status = "CAPTURED";
        payment.UpdatedAtUtc = clock.UtcNow;
        payment.AuditTrail.Add("Captured at " + payment.UpdatedAtUtc.ToString("O"));
        repo.Save(payment);
        notifier.Send(payment.CustomerEmail, "Payment captured", "Payment " + payment.Id + " was captured.");
        return new PaymentResult(true, "Capture completed", payment);
    }

    public PaymentResult Void(string id)
    {
        if (id == null || id.Trim() == "") return new PaymentResult(false, "Payment id is required", null);
        var payment = repo.Get(id.Trim());
        if (payment == null) return new PaymentResult(false, "Payment not found", null);
        if (payment.Status != "AUTHORIZED" && payment.Status != "REVIEW") return new PaymentResult(false, "Payment cannot be voided", payment);
        payment.Status = "VOIDED";
        payment.UpdatedAtUtc = clock.UtcNow;
        payment.AuditTrail.Add("Voided at " + payment.UpdatedAtUtc.ToString("O"));
        repo.Save(payment);
        notifier.Send(payment.CustomerEmail, "Payment voided", "Payment " + payment.Id + " was voided.");
        return new PaymentResult(true, "Void completed", payment);
    }

    public Payment? Find(string id)
    {
        if (id == null || id.Trim() == "") return null;
        return repo.Get(id.Trim());
    }
}
