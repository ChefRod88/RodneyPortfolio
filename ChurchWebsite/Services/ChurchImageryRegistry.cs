using System.Collections.Concurrent;

namespace ChurchWebsite.Services;

public sealed class ChurchImageryRegistry : IChurchImageryRegistry
{
    private readonly ConcurrentDictionary<string, ChurchPexelsPhoto> _photos = new(StringComparer.OrdinalIgnoreCase);

    public int LoadedCount => _photos.Count;

    public void Set(string slot, ChurchPexelsPhoto photo) =>
        _photos[slot] = photo;

    public ChurchPexelsPhoto? Get(string slot) =>
        _photos.TryGetValue(slot, out var p) ? p : null;
}
