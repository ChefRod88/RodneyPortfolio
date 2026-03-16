namespace RodneyPortfolio.Services;

/// <summary>
/// Loads resume and about content from Data/ResumeContext.txt for use in the AI system prompt.
/// </summary>
public interface IResumeContextLoader
{
    Task<string> LoadAsync(CancellationToken cancellationToken = default);
    void InvalidateCache();
}
