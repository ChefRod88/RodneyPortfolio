namespace RodneyPortfolio.Services;

/// <summary>
/// Service for generating AI-powered responses about Rodney based on resume and about content.
/// </summary>
public interface IAIChatService
{
    /// <summary>
    /// Gets a reply from the AI based on the user's question about Rodney.
    /// </summary>
    /// <param name="userMessage">The visitor's question.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of (reply text, source: "api" or "demo").</returns>
    Task<(string Reply, string Source)> GetReplyAsync(string userMessage, CancellationToken cancellationToken = default);
}
