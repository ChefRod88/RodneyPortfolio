# AGENTS.md

## Cursor Cloud specific instructions

### Overview

This repository contains two ASP.NET Core web applications and their test projects, built on **.NET 9.0**:

| Service | Port (HTTP) | Run Command |
|---------|-------------|-------------|
| **Rodney Portfolio** | 5076 | `dotnet run --project RodneyPortfolio.csproj --launch-profile http` |
| **Church Website** | 7075 | `dotnet run --project ChurchWebsite/ChurchWebsite.csproj --launch-profile http` |

Port isolation is enforced — see `.cursor/rules/port-isolation.mdc`.

### .NET SDK

The .NET 9.0 SDK is installed at `$HOME/.dotnet`. The update script handles installation automatically. `DOTNET_ROOT` and `PATH` are set in `~/.bashrc`.

### Running without external services

- **Portfolio app** gracefully degrades to an **InMemoryDatabase** when no SQL connection string is provided — core pages and the AI chatbot demo responses work fine.
- **Church Website** has zero external dependencies; all data is in-memory.
- API keys (OpenAI, Anthropic, Stripe, SMTP) are optional. Without them, the chatbot returns demo/fallback responses.

### Build, test, lint

- **Build**: `dotnet build RodneyPortfolio.sln`
- **Test (.NET)**: `dotnet test RodneyPortfolio.sln` — 35 ChurchWebsite tests + ~650 Portfolio tests. There are 8 pre-existing failing `ButtonWiringContractTests` in the Portfolio tests (CSS tokens removed from Faq.cshtml/Index.cshtml but test expectations not updated).
- **Test (JS)**: `npm run test:js` (vitest) — 7 tests for JS location route helpers.
- **Lint**: `dotnet format RodneyPortfolio.sln --verify-no-changes` — pre-existing whitespace warnings exist in the repo.

### npm dependencies

The root `package.json` includes `fsevents` (macOS-only). Use `npm install --force` on Linux to skip the platform check.

### Solution structure

The solution file `RodneyPortfolio.sln` includes all four projects. `dotnet restore` / `dotnet build` / `dotnet test` at the solution level covers everything.
