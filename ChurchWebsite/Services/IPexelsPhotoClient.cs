namespace ChurchWebsite.Services;

public interface IPexelsPhotoClient
{
    /// <summary>Returns the first photo for a search query, or null if none / error.</summary>
    Task<ChurchPexelsPhoto?> SearchFirstAsync(string query, CancellationToken cancellationToken = default);
}
