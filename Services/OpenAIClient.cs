using System.Net.Http.Json;

namespace RodneyPortfolio.Services;

public class OpenAIClient : IOpenAIClient
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public OpenAIClient(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    public Task<HttpResponseMessage> PostChatCompletionsAsync(object requestBody, CancellationToken cancellationToken = default)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key not configured.");
        }

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Remove("Authorization");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        return client.PostAsJsonAsync(
            "https://api.openai.com/v1/chat/completions",
            requestBody,
            cancellationToken);
    }
}
