namespace LegacyPayments;

public sealed class LruCache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _items = new();
    private readonly LinkedList<(TKey Key, TValue Value)> _order = new();

    public LruCache(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        _capacity = capacity;
    }

    public bool TryGet(TKey key, out TValue value)
    {
        if (!_items.TryGetValue(key, out var node))
        {
            value = default!;
            return false;
        }

        _order.Remove(node);
        _order.AddFirst(node);
        value = node.Value.Value;
        return true;
    }

    public void Put(TKey key, TValue value)
    {
        if (_items.TryGetValue(key, out var existing))
        {
            existing.Value = (key, value);
            _order.Remove(existing);
            _order.AddFirst(existing);
            return;
        }

        var node = _order.AddFirst((key, value));
        _items[key] = node;

        if (_items.Count <= _capacity)
            return;

        var leastRecent = _order.Last!;
        _order.RemoveLast();
        _items.Remove(leastRecent.Value.Key);
    }

    public void Remove(TKey key)
    {
        if (!_items.Remove(key, out var node))
            return;

        _order.Remove(node);
    }
}
