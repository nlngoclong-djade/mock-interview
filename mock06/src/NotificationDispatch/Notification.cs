namespace NotificationDispatch;
public sealed record NotificationRequest(string Recipient,string Channel,string Message);
public sealed record DispatchResult(bool Success,string Message,string? ProviderMessageId);
