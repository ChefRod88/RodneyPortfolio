using System.Net.Http.Json;

namespace RodneyPortfolio.Services;

public class AnthropicClient : IAnthropicClient
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public AnthropicClient(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    public Task<HttpResponseMessage> PostMessagesAsync(object requestBody, CancellationToken cancellationToken = default)
    {
        var apiKey = _config["Anthropic:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Anthropic API key not configured.");

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Remove("x-api-key");
        client.DefaultRequestHeaders.Remove("anthropic-version");
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        return client.PostAsJsonAsync(
            "https://api.anthropic.com/v1/messages",
            requestBody,
            cancellationToken);
    }
}
