namespace NotificationDispatch;

public interface IEmailClient { Task<string> SendAsync(string recipient,string message,CancellationToken ct=default); }
public interface ISmsClient { Task<string> SendAsync(string recipient,string message,CancellationToken ct=default); }
public interface IAuditStore { Task SaveAsync(string recipient,string channel,string providerId,CancellationToken ct=default); }

public sealed class LegacyNotificationService
{
    private readonly IEmailClient _email; private readonly ISmsClient _sms; private readonly IAuditStore _audit;
    public LegacyNotificationService(IEmailClient email,ISmsClient sms,IAuditStore audit){_email=email;_sms=sms;_audit=audit;}

    public async Task<DispatchResult> SendAsync(NotificationRequest request,CancellationToken cancellationToken=default)
    {
        if(request==null) return new(false,"Request is required",null);
        if(string.IsNullOrWhiteSpace(request.Recipient)) return new(false,"Recipient is required",null);
        if(string.IsNullOrWhiteSpace(request.Message)) return new(false,"Message is required",null);
        var channel=request.Channel?.Trim().ToUpperInvariant() ?? "";
        if(channel!="EMAIL" && channel!="SMS") return new(false,"Unsupported channel",null);
        var recipient=request.Recipient.Trim();
        string id;
        if(channel=="EMAIL") id=await _email.SendAsync(recipient,request.Message,cancellationToken);
        else id=await _sms.SendAsync(recipient,request.Message,cancellationToken);
        await _audit.SaveAsync(recipient,channel,id,cancellationToken);
        return new(true,"Notification sent",id);
    }

    public Task<DispatchResult> SendAsync(NotificationRequest request,string requestKey,CancellationToken cancellationToken=default)
        => SendAsync(request,cancellationToken);
}
