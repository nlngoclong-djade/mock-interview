namespace NotificationDispatch;

public sealed class LegacyNotificationService
{
    private readonly IAuditStore _audit;
    private readonly Validatation _validatation;
    private readonly ChannelFactory _channelFactory;

    public LegacyNotificationService(IEmailClient email, ISmsClient sms, IAuditStore audit)
    {
        _audit = audit;
        _validatation = new Validatation();
        _channelFactory = new ChannelFactory(email, sms);
    }

    public async Task<DispatchResult> SendAsync(NotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        // properties
        var channel = request.Channel?.Trim().ToUpperInvariant() ?? "";
        var recipient = request.Recipient.Trim();

        // validation request, fields
        // if the result is not null return the error message
        // if null, continue
        var result = _validatation.ValidationRequest(request, channel);
        if (result != null)
        {
            return result;
        }

        // call Channel Factory
        var id = await _channelFactory.ChannelService(channel, recipient, request.Message, cancellationToken);
        if (string.IsNullOrWhiteSpace(id))
        {
            return new(false, "Cannot get channel id", null);
        }
        else
        {
            await _audit.SaveAsync(recipient, channel, id, cancellationToken);
            return new(true, "Notification sent", id);
        }
    }

    public Task<DispatchResult> SendAsync(NotificationRequest request, string requestKey,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(request, cancellationToken);
    }
}
