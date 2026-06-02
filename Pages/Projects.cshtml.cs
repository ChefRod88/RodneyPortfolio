using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RodneyPortfolio.Pages;

public record ProjectEntry(
    string Name,
    string Description,
    string Status,        // "DEPLOYED" | "LIVE" | "IN_DEVELOPMENT"
    string[] Stack,
    string? LiveUrl,
    string? RepoUrl,
    string Year,
    string? StatusLabel = null,  // override badge text; defaults to status-based label
    string? PreviewImagePath = null
)
{
    /// <summary>Assigned automatically from list order (PROJ-001, PROJ-002, …).</summary>
    public string NodeId { get; init; } = "";
}

public class ProjectsModel : PageModel
{
    public List<ProjectEntry> Projects { get; private set; } = new();

    public void OnGet()
    {
        Projects = AssignNodeIds(BuildCatalog());
    }

    private static IReadOnlyList<ProjectEntry> BuildCatalog() =>
        new List<ProjectEntry>
        {
            new(
                Name:        "RC DEV PORTFOLIO",
                Description: "Full-stack portfolio with dual-AI assistant, job match analyzer, client portal, invoicing, Stripe payments, and CI/CD pipeline. Built to production standards with rate limiting, EF Core, and ASP.NET Core 9.",
                Status:      "DEPLOYED",
                Stack:       new[] { "C#", "ASP.NET CORE 9", "RAZOR PAGES", "EF CORE", "SQL SERVER", "AZURE", "OPENAI", "ANTHROPIC", "STRIPE", "GITHUB ACTIONS" },
                LiveUrl:     "https://www.rodneyachery.com",
                RepoUrl:     null,
                Year:        "2023"
            ),
            new(
                Name:        "NEWBETHELWINTERHAVEN.ORG",
                Description: "Interactive church website for a non-profit ministry. Features real-time location mapping, turn-by-turn routing, live service streaming, sermon archive, event listings, and Cash App donation integration. Production site on Azure with ongoing enhancements.",
                Status:      "DEPLOYED",
                Stack:       new[] { "C#", "ASP.NET CORE", "RAZOR PAGES", "LEAFLET.JS", "GRAPHHOPPER", "SIGNALR", "AZURE", "BOOTSTRAP 5" },
                LiveUrl:     "https://newbethelwinterhaven.org/",
                RepoUrl:     null,
                Year:        "2024",
                PreviewImagePath: "~/images/project_thumbnails/Screenshot 2026-06-02 071843.png"
            ),
            new(
                Name:        "PRIMEMEDICALDOCTORS.COM",
                Description: "HIPAA-aware patient booking platform for a self-pay outpatient practice accepting attorney lien (personal injury) cases. Features a two-track booking wizard (Self-Pay & Attorney Lien), 9 transactional email types via SendGrid, full EN/ES i18n, animated hero, cancellation portal, and mobile-responsive design. All PHI flows in-memory through typed C# records — never persisted server-side. Deployed on Azure App Service (West US 3) with GitHub Actions CI/CD.",
                Status:      "DEPLOYED",
                Stack:       new[] { "C#", "ASP.NET CORE 10", "RAZOR PAGES", "SENDGRID", "TWILIO", "AZURE APP SERVICE", "GITHUB ACTIONS", "CSS3", "VANILLA JS", "HTML5" },
                LiveUrl:     "https://primemedicaldoctors.com/",
                RepoUrl:     null,
                Year:        "2026",
                PreviewImagePath: "~/images/project_thumbnails/Screenshot 2026-06-02 071941.png"
            ),
            new(
                Name:        "PrimeCare EMR",
                Description: "PrimeCare EMR is a secure, HIPAA and HITECH-compliant Electronic Medical Record system engineered to strict FHIR standards. Powered by C#, Blazor, Azure, and the Firely .NET SDK, the platform reduces provider cognitive load through an intelligent \"omnibox\" patient search, a dynamic drill-down scheduling calendar, and a decluttered clinical dashboard—ensuring the technology seamlessly supports, rather than distracts from, patient care.",
                Status:      "DEPLOYED",
                Stack:       new[] { "C#", "BLAZOR", "AZURE", "FHIR", "FIRELY .NET SDK" },
                LiveUrl:     null,
                RepoUrl:     "https://github.com/ChefRod88/PMG-MedicalSystem",
                Year:        "2026",
                PreviewImagePath: "~/images/project_thumbnails/Screenshot 2026-06-01 181913.png"
            ),
        };

    private static List<ProjectEntry> AssignNodeIds(IReadOnlyList<ProjectEntry> entries) =>
        entries.Select((entry, index) => entry with { NodeId = $"PROJ-{index + 1:D3}" }).ToList();
}
