namespace RodneyPortfolio.Services;

public interface IOpenAIClient
{
    Task<HttpResponseMessage> PostChatCompletionsAsync(object requestBody, CancellationToken cancellationToken = default);
}
