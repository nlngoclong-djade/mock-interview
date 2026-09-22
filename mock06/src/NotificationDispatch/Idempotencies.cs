using System.Net;

namespace NotificationDispatch;

public sealed record Idempotencies(string Key, string Response, HttpStatusCode HttpStatusCode, string Item);