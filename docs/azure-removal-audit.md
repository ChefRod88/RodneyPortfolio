# Azure Reference Removal Audit

This document records the inventory, classification, and decisions regarding every Azure-related reference in the RodneyPortfolio repository.

## Inventory Table

| File | Line | Reference | Category | Decision | Replacement / Mitigation |
|---|---|---|---|---|---|
| [Program.cs](file:///workspaces/RodneyPortfolio/Program.cs) | 120 | `builder.Configuration.GetConnectionString("AzureSQL")` | A. Obsolete & Removable | Removed | Removed `AzureSQL` lookup; default to `DefaultConnection` only. |
| [.github/workflows/main_rodney-portfolio.yml](file:///workspaces/RodneyPortfolio/.github/workflows/main_rodney-portfolio.yml) | 1-4 | Azure deploy comments & name | A. Obsolete & Removable | Removed | Replaced with Cloudflare Pages deploy action metadata. |
| [README.md](file:///workspaces/RodneyPortfolio/README.md) | Multiple | "Azure Web App" hosting & settings | A. Obsolete & Removable | Updated | Changed descriptions to reflect Cloudflare Pages hosting. |
| [Pages/Index.cshtml](file:///workspaces/RodneyPortfolio/Pages/Index.cshtml) | 300 | `Hosting: Azure App Service` | A. Obsolete & Removable | Updated | Replaced with `Hosting: Cloudflare Pages` in transparency panel. |
| [wwwroot/index.html](file:///workspaces/RodneyPortfolio/wwwroot/index.html) | 360 | `Hosting: Azure App Service` | A. Obsolete & Removable | Updated | Replaced with `Hosting: Cloudflare Pages` to sync static output. |
| [docs/CODEBASE_EXPLAINED_SIMPLE.md](file:///workspaces/RodneyPortfolio/docs/CODEBASE_EXPLAINED_SIMPLE.md) | 90 | `Runs on Azure` | A. Obsolete & Removable | Updated | Replaced with `Runs on Cloudflare`. |
| [docs/OPENAI_API_KEY_SETUP.md](file:///workspaces/RodneyPortfolio/docs/OPENAI_API_KEY_SETUP.md) | Multiple | Azure App Settings setup | A. Obsolete & Removable | Removed | Removed Azure-specific steps; local dev uses .NET user-secrets. |
| [docs/RODNEY_PORTFOLIO_TECHNICAL_DOCUMENTATION.md](file:///workspaces/RodneyPortfolio/docs/RODNEY_PORTFOLIO_TECHNICAL_DOCUMENTATION.md) | Multiple | Azure Web App deployment details | A. Obsolete & Removable | Updated | Described the current Cloudflare Pages deploy action. |
| [Pages/Agreement.cshtml](file:///workspaces/RodneyPortfolio/Pages/Agreement.cshtml) | 59, 176 | "deployed to cloud providers including Microsoft Azure..." | C/D. Active / Legal Text | Retained | Legitimate listing of general hosting capabilities in standard legal contracts. |
| [Pages/Faq.cshtml](file:///workspaces/RodneyPortfolio/Pages/Faq.cshtml) | Multiple | "pay hosting separately on Azure, GCP, or AWS..." | C/D. Active / Legal Text | Retained | Generic educational hosting advice for prospective clients. |
| [IcmWorkspace/_config/skills/RodneyResume.md](file:///workspaces/RodneyPortfolio/IcmWorkspace/_config/skills/RodneyResume.md) | Multiple | "Azure App Service", "Azure" skills | D. Historical Resume | Retained | Legitimate record of Rodney's historical experience on his resume. |
| [Services/AnthropicChatService.cs](file:///workspaces/RodneyPortfolio/Services/AnthropicChatService.cs) | Multiple | Chatbot answers mentioning Azure skills | D. Historical Resume | Retained | Correct representation of Rodney's background in AI chatbot responses. |
| [Services/OpenAIChatService.cs](file:///workspaces/RodneyPortfolio/Services/OpenAIChatService.cs) | Multiple | Chatbot answers mentioning Azure skills | D. Historical Resume | Retained | Correct representation of Rodney's background in AI chatbot responses. |
| [Services/DualAIChatService.cs](file:///workspaces/RodneyPortfolio/Services/DualAIChatService.cs) | Multiple | Chatbot answers mentioning Azure skills | D. Historical Resume | Retained | Correct representation of Rodney's background in AI chatbot responses. |
| [Pages/Projects.cshtml.cs](file:///workspaces/RodneyPortfolio/Pages/Projects.cshtml.cs) | Multiple | "Deployed on Azure..." for other projects | C/D. Informational | Retained | Descriptions of other projects built by Rodney which reside on Azure. |
| [docs/RESUME_ATS_RECOMMENDATIONS.md](file:///workspaces/RodneyPortfolio/docs/RESUME_ATS_RECOMMENDATIONS.md) | Multiple | Resume details | D. Historical Resume | Retained | Part of historical resume text. |
| [docs/Rodney_Chery_Resume.md](file:///workspaces/RodneyPortfolio/docs/Rodney_Chery_Resume.md) | Multiple | Resume details | D. Historical Resume | Retained | Part of historical resume text. |
