using Xunit;
namespace NotificationDispatch.Tests;
public sealed class NotificationServiceTests
{
 [Fact] public async Task Email_is_sent_and_audited(){var s=Create();var r=await s.Service.SendAsync(new(" a@b.com ","email","hello"));Assert.True(r.Success);Assert.Equal(1,s.Email.Count);Assert.Equal(1,s.Audit.Count);}
 [Fact] public async Task Sms_is_sent(){var s=Create();var r=await s.Service.SendAsync(new("0909"," SMS ","hello"));Assert.True(r.Success);Assert.Equal(1,s.Sms.Count);}
 [Fact] public async Task Invalid_channel_does_not_send(){var s=Create();var r=await s.Service.SendAsync(new("x","push","hello"));Assert.False(r.Success);Assert.Equal("Unsupported channel",r.Message);}
 [Fact] public async Task Cancellation_reaches_provider(){var s=Create();s.Email.Delay=TimeSpan.FromSeconds(2);using var c=new CancellationTokenSource(20);await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>s.Service.SendAsync(new("a@b.com","email","x"),c.Token));}
 [Fact] public async Task Key_overload_preserves_success(){var s=Create();var r=await s.Service.SendAsync(new("a@b.com","email","x")," key ");Assert.True(r.Success);}
 private static Sut Create(){var e=new Email();var m=new Sms();var a=new Audit();return new(new(e,m,a),e,m,a);}
 private sealed record Sut(LegacyNotificationService Service,Email Email,Sms Sms,Audit Audit);
 private sealed class Email:IEmailClient{int n;public int Count=>n;public TimeSpan Delay{get;set;}public async Task<string>SendAsync(string r,string m,CancellationToken c=default){Interlocked.Increment(ref n);if(Delay>TimeSpan.Zero)await Task.Delay(Delay,c);return "e-"+n;}}
 private sealed class Sms:ISmsClient{int n;public int Count=>n;public Task<string>SendAsync(string r,string m,CancellationToken c=default){c.ThrowIfCancellationRequested();return Task.FromResult("s-"+Interlocked.Increment(ref n));}}
 private sealed class Audit:IAuditStore{int n;public int Count=>n;public Task SaveAsync(string r,string c,string i,CancellationToken t=default){t.ThrowIfCancellationRequested();Interlocked.Increment(ref n);return Task.CompletedTask;}}
}
