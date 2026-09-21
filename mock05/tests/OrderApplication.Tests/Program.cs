using OrderApplication;
using Xunit;

namespace OrderApplication.Tests;

public sealed class CheckoutTests
{
    [Fact]
    public async Task Stripe_checkout()
    {
        var s=Create(); var r=await s.Service.CheckoutAsync(" o-1 ","a@b.com",100,"usd","stripe");
        Assert.True(r.Success); Assert.Equal("STRIPE",r.Order!.Provider); Assert.Equal(1,s.Stripe.Count); Assert.Equal(1,s.Mail.Count);
    }

    [Fact]
    public async Task Paypal_checkout()
    {
        var s=Create(); var r=await s.Service.CheckoutAsync("o-2","a@b.com",50,"EUR","paypal");
        Assert.Equal("PAYPAL",r.Order!.Provider); Assert.Equal(1,s.PayPal.Count);
    }

    [Fact]
    public async Task Validation_preserves_first_failure()
    {
        var s=Create(); var r=await s.Service.CheckoutAsync("", "bad",0,"GBP","x");
        Assert.False(r.Success); Assert.Equal("Order id is required",r.Message); Assert.Equal(0,s.Stripe.Count);
    }

    [Fact]
    public async Task Completed_order_is_not_charged_again()
    {
        var s=Create(); await s.Service.CheckoutAsync("o-3","a@b.com",10,"USD","stripe");
        var r=await s.Service.CheckoutAsync("O-3","x@y.com",999,"EUR","paypal");
        Assert.Equal("Order already completed",r.Message); Assert.Equal(1,s.Stripe.Count); Assert.Equal(0,s.PayPal.Count);
    }

    [Fact]
    public async Task Cancellation_reaches_external_provider()
    {
        var s=Create(); s.Stripe.Delay=TimeSpan.FromSeconds(2); using var c=new CancellationTokenSource(30);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>s.Service.CheckoutAsync("o-4","a@b.com",10,"USD","stripe",c.Token));
    }

    [Fact]
    public async Task Idempotency_overload_preserves_normal_success()
    {
        var s=Create(); var r=await s.Service.CheckoutAsync("o-5","a@b.com",10,"USD","stripe"," key ");
        Assert.True(r.Success); Assert.Equal(1,s.Stripe.Count);
    }

    private static SystemUnderTest Create()
    {
        var store=new InMemoryOrderStore(); var stripe=new FakeStripe(); var paypal=new FakePayPal(); var mail=new FakeEmail();
        return new(new LegacyCheckoutService(store,stripe,paypal,mail,new FixedClock()),store,stripe,paypal,mail);
    }

    private sealed record SystemUnderTest(LegacyCheckoutService Service,InMemoryOrderStore Store,FakeStripe Stripe,FakePayPal PayPal,FakeEmail Mail);
    private sealed class FixedClock:IClock{public DateTime UtcNow=>new(2026,9,22,0,0,0,DateTimeKind.Utc);}
    private sealed class FakeStripe:IStripeClient
    {
        int _count; public int Count=>_count; public TimeSpan Delay{get;set;} public bool FailNext{get;set;}
        public async Task<string>CreateChargeAsync(string id,decimal a,string c,CancellationToken ct=default)
        {var n=Interlocked.Increment(ref _count);if(Delay>TimeSpan.Zero)await Task.Delay(Delay,ct);if(FailNext){FailNext=false;throw new InvalidOperationException("charge failed");}return "stripe-"+n;}
    }
    private sealed class FakePayPal:IPayPalClient
    {
        int _count; public int Count=>_count;
        public async Task<string>PayAsync(string id,decimal a,string c,CancellationToken ct=default)
        {await Task.Yield();ct.ThrowIfCancellationRequested();return "paypal-"+Interlocked.Increment(ref _count);}
    }
    private sealed class FakeEmail:IEmailSender
    {
        int _count; public int Count=>_count;
        public Task SendAsync(string e,string s,string b,CancellationToken ct=default)
        {ct.ThrowIfCancellationRequested();Interlocked.Increment(ref _count);return Task.CompletedTask;}
    }
}
