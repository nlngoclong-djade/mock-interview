using System.Net;

namespace NotificationDispatch;

public sealed class LegacyNotificationService
{
    private readonly IAuditStore _audit;
    private readonly Validatation _validatation;
    private readonly ChannelFactory _channelFactory;
    private readonly IdempotencyService _idempotencyService;

    public LegacyNotificationService(IEmailClient email, ISmsClient sms, IAuditStore audit)
    {
        _audit = audit;
        _validatation = new Validatation();
        _channelFactory = new ChannelFactory(email, sms);
        _idempotencyService = new IdempotencyService();
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

    public async Task<DispatchResult> SendAsync(NotificationRequest request, string requestKey,
        CancellationToken cancellationToken = default)
    {
        // need to check idempotency key
        // check key if matched with existed key then return
        // need to implement concurrency
        // cases:
        // 2 requests go in almost same time, need to check version before do anything
        var key = requestKey.ToLower() + "-" + request.Recipient.ToLower();
        // true if existed key
        var result = _idempotencyService.CheckKey(key);
        if (result != null)
        {
            // check http status, response
            switch(result.HttpStatusCode)
            {
                // already sent
                case HttpStatusCode.Accepted:
                    return new(true, "Notification sent", result.Response);
                case HttpStatusCode.BadRequest:
                    return new(false, "Notification failed", result.Response);
                case HttpStatusCode.Conflict:
                    return new(false, "Conflict with other channel", result.Response);
            }
        }
        // false if not existed key
        // run sent notify first
        var response = await SendAsync(request, cancellationToken);
        
        // add idempotent key
        var idem = new Idempotencies(requestKey, response.Message, HttpStatusCode.Accepted, request.Recipient);
        _idempotencyService.SaveKey(idem);
        return response;
    }
}
