namespace RodneyPortfolio.Services;

/// <summary>
/// Loads resume and about content from Data/ResumeContext.txt for use in the AI system prompt.
/// </summary>
public interface IResumeContextLoader
{
    Task<string> LoadAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Loads resume context from the Data folder. Update Data/ResumeContext.txt when your resume changes.
/// </summary>
public class ResumeContextLoader : IResumeContextLoader
{
    private readonly IWebHostEnvironment _env;
    private string? _cachedContext;

    public ResumeContextLoader(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedContext != null)
            return _cachedContext;

        var path = Path.Combine(_env.ContentRootPath, "Data", "ResumeContext.txt");
        if (!File.Exists(path))
            return "Resume context file not found. Please add Data/ResumeContext.txt with your resume and about content.";

        _cachedContext = await File.ReadAllTextAsync(path, cancellationToken);
        return _cachedContext;
    }
}
