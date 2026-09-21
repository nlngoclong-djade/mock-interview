using LegacyPayments.Interface;
using LegacyPayments.Repositories;

namespace LegacyPayments;

public class LegacyPaymentProcessor
{
    private readonly IPaymentDataStore _repo;
    private readonly IReceiptSender _sender;
    private readonly DateTime _clock;

    public LegacyPaymentProcessor(IPaymentDataStore repo, IReceiptSender sender, DateTime clock)
    {
        this._repo = repo;
        this._sender = sender;
        this._clock = clock;
    }

    public PaymentResult MakePayment(string id, string email, decimal amount, string currency, bool priorityCustomer)
    {
        if (id == null || id.Trim() == "") return new PaymentResult(false, "Payment id is required", null);
        if (email == null || email.Trim() == "" || !email.Contains('@')) return new PaymentResult(false, "A valid email is required", null);
        if (amount <= 0) return new PaymentResult(false, "Amount must be positive", null);

        var c = currency == null ? "" : currency.Trim().ToUpperInvariant();
        if (c != "USD" && c != "EUR" && c != "GBP") return new PaymentResult(false, "Unsupported currency", null);
        if (_repo.Load(id) != null) return new PaymentResult(false, "Payment already exists", null);

        decimal fee;
        if (c == "USD") fee = amount * 0.029m + 0.30m;
        else if (c == "EUR") fee = amount * 0.025m + 0.25m;
        else fee = amount * 0.027m + 0.20m;
        if (priorityCustomer) fee = fee * 0.80m;
        fee = Math.Round(fee, 2, MidpointRounding.AwayFromZero);

        var p = new Payment();
        p.Id = id.Trim();
        p.CustomerEmail = email.Trim();
        p.Amount = amount;
        p.Currency = c;
        p.Status = amount >= 5000 ? "REVIEW" : "PAID";
        p.Fee = fee;
        p.UpdatedAtUtc = _clock;
        _repo.Save(p);

        if (p.Status == "PAID")
            _sender.Send(p.CustomerEmail, "Payment received", "Payment " + p.Id + " for " + p.Amount.ToString("0.00") + " " + p.Currency + " was received. Fee: " + p.Fee.ToString("0.00"));
        else
            _sender.Send(p.CustomerEmail, "Payment under review", "Payment " + p.Id + " is being reviewed.");

        return new PaymentResult(true, p.Status == "PAID" ? "Payment completed" : "Payment requires review", p);
    }

    public PaymentResult Refund(string id, decimal amount)
    {
        if (id == null || id.Trim() == "") return new PaymentResult(false, "Payment id is required", null);
        if (amount <= 0) return new PaymentResult(false, "Amount must be positive", null);
        var p = _repo.Load(id.Trim());
        if (p == null) return new PaymentResult(false, "Payment not found", null);
        if (p.Status != "PAID") return new PaymentResult(false, "Only paid payments can be refunded", p);
        if (amount > p.Amount) return new PaymentResult(false, "Refund exceeds payment amount", p);

        p.Status = amount == p.Amount ? "REFUNDED" : "PARTIALLY_REFUNDED";
        p.UpdatedAtUtc = _clock;
        _repo.Save(p);
        _sender.Send(p.CustomerEmail, "Refund processed", "Refund of " + amount.ToString("0.00") + " " + p.Currency + " for payment " + p.Id + " was processed.");
        return new PaymentResult(true, "Refund completed", p);
    }

    public PaymentResult Cancel(string id)
    {
        if (id == null || id.Trim() == "") return new PaymentResult(false, "Payment id is required", null);
        var p = _repo.Load(id.Trim());
        if (p == null) return new PaymentResult(false, "Payment not found", null);
        if (p.Status != "REVIEW") return new PaymentResult(false, "Only payments under review can be cancelled", p);
        p.Status = "CANCELLED";
        p.UpdatedAtUtc = _clock;
        _repo.Save(p);
        _sender.Send(p.CustomerEmail, "Payment cancelled", "Payment " + p.Id + " was cancelled.");
        return new PaymentResult(true, "Cancellation completed", p);
    }

    public Payment? Find(string id)
    {
        if (id == null || id.Trim() == "") return null;
        return _repo.Load(id.Trim());
    }
}
