namespace NotificationDispatch;

public class IdempotencyService
{
    private Dictionary<string, Idempotencies> _listIdems = new Dictionary<string, Idempotencies>();
    
    public IdempotencyService()
    {
        
    }

    public Idempotencies? CheckKey(string key)
    {
        return _listIdems.TryGetValue(key, out var value) ? value : null;
    }
    
    public void SaveKey(Idempotencies idem)
    {
        var key = idem.Key.ToLower() + "-" + idem.Item.ToLower();
        _listIdems.Add(key, idem);
    }
}