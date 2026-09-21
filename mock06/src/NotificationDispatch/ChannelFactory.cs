namespace NotificationDispatch;

public sealed class ChannelFactory
{
    private readonly IEmailClient _email; 
    private readonly ISmsClient _sms; 
    public ChannelFactory(IEmailClient email, ISmsClient sms)
    {
        _email = email;
        _sms = sms;
    }
    
    public async Task<string> ChannelService(string channel, string recipient, string message, CancellationToken cancellationToken)
    {
        if (channel ==  Constants.Channel.EMAIL.ToString())
            return await _email.SendAsync(recipient, message, cancellationToken);
        else if  (channel == Constants.Channel.SMS.ToString())
            return await _sms.SendAsync(recipient, message, cancellationToken);
        else
            return "";
    }
}