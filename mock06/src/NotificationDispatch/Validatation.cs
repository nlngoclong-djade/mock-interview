namespace NotificationDispatch;

public sealed  class Validatation
{
    public DispatchResult? ValidationRequest(NotificationRequest request, string channel)
    {
        if (request == null)
            return new(false, "Request is required", null);
        
        if (string.IsNullOrWhiteSpace(request.Recipient))
            return new(false, "Recipient is required", null);
        
        if (string.IsNullOrWhiteSpace(request.Message))
            return new(false, "Message is required", null);
        
        if (channel != Constants.Channel.EMAIL.ToString() && channel !=  Constants.Channel.SMS.ToString())
            return new(false, "Unsupported channel", null);
        
        return null;
    }
}