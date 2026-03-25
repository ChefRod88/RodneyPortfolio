using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RodneyPortfolio.Pages;

public record ProjectEntry(
    string NodeId,
    string Name,
    string Description,
    string Status,        // "DEPLOYED" | "LIVE" | "IN_DEVELOPMENT"
    string[] Stack,
    string? LiveUrl,
    string? RepoUrl,
    string Year,
    string? StatusLabel = null  // override badge text; defaults to status-based label
);

public class ProjectsModel : PageModel
{
    public List<ProjectEntry> Projects { get; private set; } = new();

    public void OnGet()
    {
        Projects = new List<ProjectEntry>
        {
            new(
                NodeId:      "PROJ-001",
                Name:        "RC DEV PORTFOLIO",
                Description: "Full-stack portfolio with dual-AI assistant, job match analyzer, client portal, invoicing, Stripe payments, and CI/CD pipeline. Built to production standards with rate limiting, EF Core, and ASP.NET Core 9.",
                Status:      "DEPLOYED",
                Stack:       new[] { "C#", "ASP.NET CORE 9", "RAZOR PAGES", "EF CORE", "SQL SERVER", "AZURE", "OPENAI", "ANTHROPIC", "STRIPE", "GITHUB ACTIONS" },
                LiveUrl:     "https://www.rodneyachery.com",
                RepoUrl:     null,
                Year:        "2023"
            ),
            new(
                NodeId:      "PROJ-002",
                Name:        "NEW BETHEL CHURCH WEBSITE",
                Description: "Interactive church website for a non-profit ministry. Features real-time location mapping, turn-by-turn routing, live service streaming, sermon archive, event listings, and Cash App donation integration. Currently undergoing full refactor.",
                Status:      "IN_DEVELOPMENT",
                Stack:       new[] { "C#", "ASP.NET CORE", "RAZOR PAGES", "LEAFLET.JS", "GRAPHHOPPER", "SIGNALR", "AZURE", "BOOTSTRAP 5" },
                LiveUrl:     null,
                RepoUrl:     null,
                Year:        "2024",
                StatusLabel: "◌ REFACTOR IN PROGRESS"
            ),
            new(
                NodeId:      "PROJ-003",
                Name:        "MINI-D365-CRM",
                Description: "Enterprise-grade CRM API modeled after Microsoft Dynamics 365. Implements OData v4 query protocol, EF Core repository pattern, full xUnit test suite, and Swagger documentation. Live on Azure.",
                Status:      "DEPLOYED",
                Stack:       new[] { "C#", "ASP.NET CORE 10", "ODATA V4", "EF CORE", "SQL SERVER", "XUNIT", "MOQ", "AZURE", "SWAGGER" },
                LiveUrl:     "https://mini-d365-crm-duaycqf8bvc4ehcr.westus3-01.azurewebsites.net/swagger/index.html",
                RepoUrl:     null,
                Year:        "2025"
            ),
            new(
                NodeId:      "PROJ-004",
                Name:        "ASK RODNEY AI CHATBOT",
                Description: "Dual-AI orchestration system running OpenAI GPT-4o-mini and Anthropic Claude in parallel. Merges responses into a single richer answer with graceful fallback, rate limiting, and resume-grounded context.",
                Status:      "LIVE",
                Stack:       new[] { "C#", "OPENAI API", "ANTHROPIC API", "DUAL-AI ORCHESTRATION", "ASP.NET CORE", "DEPENDENCY INJECTION" },
                LiveUrl:     "https://www.rodneyachery.com/#ask-rodney",
                RepoUrl:     null,
                Year:        "2024"
            ),
            new(
                NodeId:      "PROJ-005",
                Name:        "JOB MATCH ANALYZER",
                Description: "Paste any job description and get an AI-powered match score, aligned skills, gaps analysis, and interview talking points. Runs dual-provider analysis (OpenAI + Anthropic) and merges results for accuracy.",
                Status:      "LIVE",
                Stack:       new[] { "C#", "OPENAI API", "ANTHROPIC API", "JSON PARSING", "ASP.NET CORE", "JAVASCRIPT" },
                LiveUrl:     "https://www.rodneyachery.com/#ask-rodney",
                RepoUrl:     null,
                Year:        "2025"
            ),
            new(
                NodeId:      "PROJ-006",
                Name:        "STUDENT PROGRESS TRACKER",
                Description: "Cross-platform mobile and desktop app for tracking academic progress. WGU Software Engineering capstone project with .NET MAUI front-end, ASP.NET Core Web API backend, Azure SQL database, and full xUnit coverage.",
                Status:      "IN_DEVELOPMENT",
                Stack:       new[] { ".NET MAUI", "C#", "ASP.NET CORE WEB API", "AZURE SQL", "EF CORE", "XUNIT", "MVVM" },
                LiveUrl:     null,
                RepoUrl:     null,
                Year:        "2026"
            )
        };
    }
}
