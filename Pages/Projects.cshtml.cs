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
                Stack:       new[] { "C#", "ASP.NET CORE 9", "ASP.NET CORE RAZOR PAGES", "ENTITY FRAMEWORK CORE", "MICROSOFT SQL SERVER", "MICROSOFT AZURE", "OPENAI API", "ANTHROPIC API", "STRIPE API", "GITHUB ACTIONS" },
                LiveUrl:     "https://www.rodneyachery.com",
                RepoUrl:     "https://github.com/ChefRod88/RodneyPortfolio",
                Year:        "2026"
            ),
            new(
                Name:        "NEWBETHELWINTERHAVEN.ORG",
                Description: "Interactive church website for a non-profit ministry. Features real-time location mapping, turn-by-turn routing, live service streaming, sermon archive, event listings, and Cash App donation integration. Production site on Azure with ongoing enhancements.",
                Status:      "DEPLOYED",
                Stack:       new[] { "C#", "ASP.NET CORE", "ASP.NET CORE RAZOR PAGES", "LEAFLET.JS", "GRAPHHOPPER API", "ASP.NET CORE SIGNALR", "MICROSOFT AZURE", "BOOTSTRAP 5" },
                LiveUrl:     "https://newbethelwinterhaven.org/",
                RepoUrl:     "https://gist.github.com/ChefRod88/7ca9cf649bde7952544759996cf0a7c9",
                Year:        "2026",
                PreviewImagePath: "~/images/project_thumbnails/Screenshot 2026-06-02 071843.png"
            ),
            new(
                Name:        "PRIMEMEDICALDOCTORS.COM",
                Description: "HIPAA-aware patient booking platform for a self-pay outpatient practice accepting attorney lien (personal injury) cases. Features a two-track booking wizard (Self-Pay & Attorney Lien), 9 transactional email types via SendGrid, full EN/ES i18n, animated hero, cancellation portal, and mobile-responsive design. All PHI flows in-memory through typed C# records — never persisted server-side. Deployed on Azure App Service (West US 3) with GitHub Actions CI/CD.",
                Status:      "DEPLOYED",
                Stack:       new[] { "C#", "ASP.NET CORE 10", "ASP.NET CORE RAZOR PAGES", "TWILIO SENDGRID", "TWILIO API", "MICROSOFT AZURE APP SERVICE", "GITHUB ACTIONS", "CSS3", "VANILLA JAVASCRIPT", "HTML5" },
                LiveUrl:     "https://primemedicaldoctors.com/",
                RepoUrl:     "https://gist.github.com/ChefRod88/7b0c3386a13f6693132a78b3398587f2",
                Year:        "2026",
                PreviewImagePath: "~/images/project_thumbnails/Screenshot 2026-06-02 071941.png"
            ),
            new(
                Name:        "PrimeCare EMR",
                Description: "PrimeCare EMR is a secure custom SaaS platform built for **[Prime Medical Group](https://www.primemedicaldoctors.com), HIPAA and HITECH-compliant Electronic Medical Record system engineered to strict FHIR standards. Powered by C#, Blazor, Azure, and the Firely .NET SDK, the platform reduces provider cognitive load through an intelligent \"omnibox\" patient search, a dynamic drill-down scheduling calendar, and a decluttered clinical dashboard—ensuring the technology seamlessly supports, rather than distracts from, patient care.",
                Status:      "DEPLOYED",
                Stack:       new[] { "C#", "ASP.NET CORE BLAZOR", "MICROSOFT AZURE", "HL7 FHIR", "FIRELY .NET SDK" },
                LiveUrl:     null,
                RepoUrl:     "https://gist.github.com/ChefRod88/d2d256cde587fe00b9d6b339b5a9139c",
                Year:        "2026",
                PreviewImagePath: "~/images/project_thumbnails/Screenshot 2026-06-01 181913.png"
            ),
            new(
                Name:        "MEDEX FOR VETS PORTAL",
                Description: "MedEx for Vets Portal is a custom SaaS platform built for **[MedEx for Vets](https://www.medexvets.com), HIPAA-compliant patient and provider portal designed to allow veterans (claimants) to prepare for their Independent Medical Exams (IME) and authorize MedEx staff to audit records. The system features a Claimant Portal, a Provider/Admin Ops Portal, and a shared secure database context.",
                Status:      "DEPLOYED",
                Stack:       new[] { "C#", ".NET 10", "ASP.NET CORE BLAZOR", "BLAZOR WEB APP", "MICROSOFT AZURE BLOB STORAGE", "AZURE SQL DATABASE", "HIPAA COMPLIANCE", "FIRELY .NET SDK", "DOCUSIGN ESIGNATURE API", "CALENDLY API V2" },
                LiveUrl:     null,
                RepoUrl:     "https://gist.github.com/ChefRod88/0e26304e85c3b2108dae940732aca497",
                Year:        "2026",
                PreviewImagePath: "~/images/project_thumbnails/Black and White Minimalist Aesthetic Photo Collage Fashion Collection Instagram Post.png"
            ),
        };

    private static List<ProjectEntry> AssignNodeIds(IReadOnlyList<ProjectEntry> entries) =>
        entries.Select((entry, index) => entry with { NodeId = $"PROJ-{index + 1:D3}" }).ToList();
}
