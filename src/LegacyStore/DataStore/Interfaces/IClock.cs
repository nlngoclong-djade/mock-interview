namespace LegacyStore.DataStore.Interfaces;

public interface IClock
{
    DateTime UtcNow { get; }
}