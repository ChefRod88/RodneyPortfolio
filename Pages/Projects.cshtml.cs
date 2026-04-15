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
                Name:        "PRIME MEDICAL GROUP",
                Description: "HIPAA-aware patient booking platform for a self-pay outpatient practice accepting attorney lien (personal injury) cases. Features a two-track booking wizard (Self-Pay & Attorney Lien), 9 transactional email types via SendGrid, full EN/ES i18n, animated hero, cancellation portal, and mobile-responsive design. All PHI flows in-memory through typed C# records — never persisted server-side. Deployed on Azure App Service (West US 3) with GitHub Actions CI/CD.",
                Status:      "LIVE",
                Stack:       new[] { "C#", "ASP.NET CORE 10", "RAZOR PAGES", "SENDGRID", "TWILIO", "AZURE APP SERVICE", "GITHUB ACTIONS", "CSS3", "VANILLA JS", "HTML5" },
                LiveUrl:     "https://primemedicaldoctors.com/",
                RepoUrl:     null,
                Year:        "2026"
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
            ),
            new(
                NodeId:      "PROJ-007",
                Name:        "DRUMGO PUBLISHING WEBSITE",
                Description: "Full website refactor for Drumgo Publishing. Modernizing architecture, design, and content delivery for an independent publishing brand.",
                Status:      "IN_DEVELOPMENT",
                Stack:       new[] { "C#", "ASP.NET CORE", "RAZOR PAGES", "CSS3", "AZURE" },
                LiveUrl:     null,
                RepoUrl:     null,
                Year:        "2025",
                StatusLabel: "◌ REFACTOR IN PROGRESS"
            ),
            new(
                NodeId:      "PROJ-008",
                Name:        "FLORIDA THEOLOGICAL SEMINARY & BIBLE COLLEGE",
                Description: "Full website refactor for Florida Theological Seminary & Bible College. Rebuilding and modernizing the web presence for an accredited institution serving the Lakeland, FL community.",
                Status:      "IN_DEVELOPMENT",
                Stack:       new[] { "C#", "ASP.NET CORE", "RAZOR PAGES", "CSS3", "AZURE" },
                LiveUrl:     "https://www.ftslakeland.org",
                RepoUrl:     null,
                Year:        "2025",
                StatusLabel: "◌ REFACTOR IN PROGRESS"
            )
        };
    }
}
