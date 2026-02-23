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
    /// <returns>The AI-generated reply.</returns>
    Task<string> GetReplyAsync(string userMessage, CancellationToken cancellationToken = default);
}
