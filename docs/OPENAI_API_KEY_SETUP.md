# How I Set Up My OpenAI API Key Safely

I configure my portfolio chatbot to use the OpenAI API without ever storing my key in my codebase. Here's how I did it—and how you can do the same.

---

## Environment Setup

Local and production environments use different methods for configuration:

| Environment | Method | Where it lives |
|-------------|--------|----------------|
| **Localhost** | User Secrets | Your machine (`dotnet user-secrets set`) |
| **Production** | Cloudflare Workers Secrets (Future) | Cloudflare Edge Environment |

---

## Why I Care About This

I never want my API key in a file that gets committed to Git. If someone gets my repo, they shouldn't get my key. I use **User Secrets** for local development. The key flows from secure storage into my app at runtime—never from a config file in the repo.

---

## How My .NET App Reads the Key Locally

I don't hardcode anything. My `OpenAIChatService` reads from configuration:

```csharp
var apiKey = _config["OpenAI:ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey))
    return "The chatbot is not configured.";
// Call OpenAI API with apiKey
```

| Source            | When It's Used |
| ----------------- | -------------- |
| User Secrets      | Development (Localhost) |
| appsettings.json  | Fallback (I keep `ApiKey` empty there) |

---

## Quick Reference for Local Development

| Task                    | Command or Action |
| ----------------------- | ----------------- |
| Set key locally         | `dotnet user-secrets set "OpenAI:ApiKey" "sk-..."` |
| List secrets            | `dotnet user-secrets list` |
| Run app                 | `dotnet run` |
