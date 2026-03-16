namespace RodneyPortfolio.Services;

public interface IAnthropicClient
{
    Task<HttpResponseMessage> PostMessagesAsync(object requestBody, CancellationToken cancellationToken = default);
}
