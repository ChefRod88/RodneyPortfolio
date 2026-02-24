# AGENTS.md

## Cursor Cloud specific instructions

### Project overview

ASP.NET Core 10 portfolio website for Rodney Chery. Single required service — no databases or external services needed. The OpenAI API key is optional; without it the chatbot returns a "not configured" message (demo fallback only triggers on API call failure, not missing key).

### Prerequisites

.NET 10 SDK must be installed. On Ubuntu 24.04, install via: `sudo add-apt-repository -y ppa:dotnet/previews && sudo apt-get update && sudo apt-get install -y dotnet-sdk-10.0`.

### Case-sensitivity gotcha

The solution file (`RodneyPortfolio.sln`) references the test project as `Tests\RodneyPortfolio.Tests.csproj` (uppercase `T`), but the actual directory is `tests/` (lowercase). On Linux this breaks `dotnet restore` at the solution level. Workaround: build and test the projects directly instead of via the solution:

```sh
dotnet build RodneyPortfolio.csproj
dotnet test tests/RodneyPortfolio.Tests.csproj
```

### Running the app

```sh
dotnet run --launch-profile http
```

Listens on `http://localhost:5076`. See `Properties/launchSettings.json` for all profiles.

### Running tests

```sh
dotnet test tests/RodneyPortfolio.Tests.csproj
```

21 xUnit tests covering `InputValidator`, `ContentFilter`, and `OpenAIChatService`.

### Key API endpoints

- `POST /api/chat` — `{ "message": "..." }` → `{ "reply": "..." }`
- `POST /api/chat/job-match` — `{ "jobDescription": "..." }` → match score, skills, gaps, talking points

### ChurchWebsite (secondary, excluded from main build)

Located in `ChurchWebsite/`. Excluded via `<Compile Remove="ChurchWebsite/**" />` in the main `.csproj`. Run separately if needed: `dotnet run --project ChurchWebsite/ChurchWebsite.csproj`.
