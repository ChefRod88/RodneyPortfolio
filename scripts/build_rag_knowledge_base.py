#!/usr/bin/env python3
"""
Build Multi-Document Vector RAG Knowledge Base for Rodney Chery Portfolio.
Generates 512-dimensional vector embeddings using OpenAI text-embedding-3-small
across Resume, Technical Articles, Services & Compliance, Projects, and FAQs.
Saves the compiled vector store to worker/rag-index.json.
"""

import os
import json
import urllib.request
import urllib.error

CHUNKS = [
    # ── RESUME & CAREER HISTORY ──
    {
        "id": "resume-summary-profile",
        "category": "Profile & Summary",
        "title": "Rodney Chery Professional Summary & Overview",
        "source": "Official Resume",
        "url": "/#about",
        "text": """Rodney Amos Chery is a Technical Support Specialist III at LeadVenture and Full-Stack Developer with 3+ years of enterprise IT experience and hands-on web development skills in JavaScript (ES6+), HTML5, CSS3/LESS, Bootstrap, and C# / .NET.
Experienced in managing high-volume ticket queues via phone, email, and chat in strict SLA-driven environments. Comfortable making live changes to client-facing web properties — diagnosing layout and functionality issues, engineering targeted fixes, and communicating solutions clearly to non-technical customers.
Holds AWS Certified Cloud Practitioner, ITIL 4 Foundation, and Google IT Support certifications. Currently pursuing a B.S. in Software Engineering at Western Governors University (WGU, Expected Dec 2026). Location: Winter Haven, FL (available for remote and on-site opportunities)."""
    },
    {
        "id": "resume-leadventure-exp",
        "category": "Career Experience",
        "title": "Technical Support Specialist III at LeadVenture (Dealer Spike)",
        "source": "Official Resume",
        "url": "/#about",
        "text": """LeadVenture (June 2026 – Present, Remote) — Technical Support Specialist III:
Supports LeadVenture's Dealer Spike SaaS platform, delivering Tier III production support and front-end engineering for live automotive, marine, and power-sports dealership websites in production.
- Diagnoses production defects across Dealer Spike's CMS, Classic ASP server-rendered architecture, VLP/VDP inventory systems, dynamic DOM rendering, third-party integrations, analytics, SEO, and customer-facing functionality.
- Performs root-cause analysis using Chrome DevTools, Console, Network inspection, DOM analysis, HTTP behavior, event tracing, and responsive testing.
- Engineers production customizations using JavaScript, jQuery, HTML5, CSS3/LESS, Bootstrap, DOM APIs, MutationObserver, delegated events, Fetch/AJAX patterns, and asynchronous UI logic.
- Builds and modifies inventory pricing, payment calculations, filters, promotional components, forms, navigation, CTAs, disclosures, modals, landing pages, and responsive inventory experiences.
- Works with .asp pages, CMS snippets, templates, configuration files, dynamic inventory markup, and dealer-specific settings. Deploys code across staging and production using VPN, FTP/SFTP, and VS Code.
- Troubleshoots GTM, GA4, Google Ads, Tealium, Cloudflare, TrustArc, AudioEye, lead tools, and external applications. Implements and diagnoses JSON-LD, Schema.org, canonical URLs, redirects, indexing directives, metadata, crawler behavior, and HTTP status codes.
- Owns Salesforce cases through investigation, implementation, QA, escalation, and resolution; creates Jira escalations with reproduction steps, affected URLs, and business impact."""
    },
    {
        "id": "resume-canon-exp",
        "category": "Career Experience",
        "title": "Technical Support Specialist at Canon Information Technology Services",
        "source": "Official Resume",
        "url": "/#about",
        "text": """Canon Information Technology Services (March 2025 – June 2026, Remote) — Technical Support Specialist:
- Managed 15–25 enterprise support incidents per shift via phone, email, and ticketing system in a high-volume SLA-driven environment.
- Triaged, categorized, and prioritized incoming requests — determining fastest resolution path for each issue type and customer segment.
- Diagnosed and resolved software, hardware, Windows OS, and connectivity issues for enterprise end users across diverse environments.
- Communicated technical solutions clearly to non-technical users, managing expectations through resolution and following up to confirm fix.
- Documented resolutions and authored knowledge base articles and how-to guides that reduced repeat incident volume across the team.
- Escalated complex issues to engineering teams with clear documentation and owned the customer communication loop."""
    },
    {
        "id": "resume-freelance-church-exp",
        "category": "Career Experience",
        "title": "Freelance Full-Stack Developer & Ministry of Technology",
        "source": "Official Resume",
        "url": "/Projects",
        "text": """Freelance Full-Stack Developer (Feb 2023 – Present) & Minister of Technology at New Bethel Missionary Baptist Church (2025 – Present):
- Built complete web applications from scratch using HTML, CSS, JavaScript, and C# / ASP.NET Core.
- Built RESTful and OData v4 APIs deployed to Azure App Service and Cloudflare with GitHub Actions CI/CD pipelines.
- Designed relational SQL Server and SQLite databases with stored procedures, foreign key relationships, and aggregate queries.
- Built and maintains live production web application for church congregation featuring dynamic event management, responsive media, and YouTube livestream integrations."""
    },
    {
        "id": "resume-technical-skills",
        "category": "Technical Skillset",
        "title": "Comprehensive Technical Skillset & Stack",
        "source": "Official Resume",
        "url": "/#services",
        "text": """Rodney Chery Technical Stack & Core Competencies:
- Languages & Core Web: JavaScript (ES6+), jQuery, HTML5, CSS3/LESS, Bootstrap, DOM Manipulation, DOM APIs, MutationObserver, Classic ASP, C#, .NET, .NET MAUI, REST APIs, JSON, SQL, SQLite
- Cloud, Platforms & DevOps: Cloudflare Workers, Cloudflare Pages, AWS, Git, GitHub, GitHub Actions, Dealer Spike CMS, FTP/SFTP, VS Code, Windows OS
- Analytics & Technical SEO: Chrome DevTools, JSON-LD, Schema.org Structured Data, Google Tag Manager (GTM), Google Analytics 4 (GA4), Tealium, Technical SEO, Canonical URLs, 301/302 Redirects, Crawler Behavior, HTTP Status Codes
- Enterprise Systems: Salesforce Service Cloud, Jira, Active Directory, Wireshark, Incident Resolution, SLA Management"""
    },

    # ── TECHNICAL ARTICLES ──
    {
        "id": "article-support-workflow-architecture",
        "category": "Technical Article",
        "title": "AI-Assisted Support Engineering: Architecture & Dual-Plane Separation",
        "source": "Article: AI-Assisted Support Engineering Workflow",
        "url": "/Articles/ai-assisted-support-engineering-workflow",
        "text": """How I Built an AI-Assisted Support Engineering Workflow for Troubleshooting Production Websites Safely:
In multi-tenant dealership web platforms running legacy Classic ASP (.asp) backends and deeply nested server-side includes, ad-hoc in-place editing on production SFTP servers creates severe risks: catastrophic site downtime, irreversible code loss, and uncontained blast radiuses.
The system decouples into two strict operational planes:
1. Control Plane: Manages case metadata, environmental verification, confirmation gates, and agentic stage boundaries.
2. Data Plane: Manages the actual dealership code assets, scripts, stylesheets, and markup.
Tripartite Operational Separation:
- Deterministic Automation (PowerShell & WinSCP .NET): Executes mechanical scaffolding, host key validation, SFTP transport, and Git initialization. Never makes heuristic guesses.
- Interpretive Analysis (AI / LLMs): Functions as a read-only code reviewer, dependency tracer, and diff analyzer within sandboxed stages.
- Human Authority (Support Engineer): Retains exclusive authority over hypothesis validation, code changes, testing, and deployment."""
    },
    {
        "id": "article-support-workflow-powershell",
        "category": "Technical Article",
        "title": "PowerShell & WinSCP Automation in Support Engineering",
        "source": "Article: AI-Assisted Support Engineering Workflow",
        "url": "/Articles/ai-assisted-support-engineering-workflow",
        "text": """PowerShell Control Plane and WinSCP .NET Automation:
To prevent accidental cross-environment execution, tooling is explicitly separated into Get-LiveSite.ps1 and Get-StageSite.ps1 rather than a single script with flags.
- Uses the WinSCP .NET assembly (WinSCPnet.dll) to execute programmatic SFTP transfers with pinned SSH host key fingerprints.
- Establishes immutable local baselines: files downloaded from production are duplicated into a read-only baseline directory and a separate active Git repository workspace.
- Guarantees zero accidental overwrites: local modifications are isolated and tracked with local Git commits before any deployment is even considered.
Core axiom: LOCAL COPY != PRODUCTION. Local test success does not guarantee production behavior, and local failure does not disprove customer bug reports."""
    },
    {
        "id": "article-support-workflow-icm",
        "category": "Technical Article",
        "title": "Interpretable Context Methodology (ICM) Agentic Stages",
        "source": "Article: AI-Assisted Support Engineering Workflow",
        "url": "/Articles/ai-assisted-support-engineering-workflow",
        "text": """Interpretable Context Methodology (ICM) for Support Engineering:
Instead of pasting raw snippets into open chat windows where models hallucinate, the workflow uses filesystem-based stage directories:
- Stage 01 Intake: Ingests raw ticket descriptions, error logs, and customer reproduction steps into structured intake markdown.
- Stage 02 Investigation: Dispatches read-only LLM subagents to trace dependencies, inspect server-side includes, and locate cascading CSS rules.
- Stage 03 Validation: Reviews prospective git diffs against the immutable baseline. Generates verification checklists and test scenarios.
Human-Gated Deployment: Changes must be explicitly reviewed and deployed by the human engineer through verified staging environments before production upload."""
    },
    {
        "id": "article-ssg-cloudflare-pipeline",
        "category": "Technical Article",
        "title": "Zero-Cost ASP.NET Core: Static Site Generation on Cloudflare Pages",
        "source": "Article: Zero-Cost ASP.NET Core",
        "url": "/Articles/zero-dollar-aspnetcore-cloudflare-ssg",
        "text": """Zero-Cost ASP.NET Core: Building a Markdown Blog on Cloudflare Pages:
Engineers often face a dilemma: paying $20-$50/month to host ASP.NET Core on Microsoft Azure vs. using simple static files.
Rodney engineered a hybrid solution combining the best of both worlds:
1. ASP.NET Core Backend: Uses Markdig and YamlDotNet NuGet packages to parse YAML front-matter (SEO metadata, keywords, dates) and render Markdown to clean HTML.
2. Automated SSG Build Pipeline: A Python scraping pipeline boots the backend locally, crawls the sitemap (/sitemap.xml), captures all rendered pages with JSON-LD and Open Graph tags, and freezes them to static wwwroot/ assets.
3. Zero-Cost Edge Hosting: Deployed globally across Cloudflare edge servers with Cloudflare Pages / Workers for exactly $0/month while achieving 100/100 Lighthouse performance and SEO scores."""
    },

    # ── SERVICES & CLIENT AGREEMENT ──
    {
        "id": "services-agreement-scope",
        "category": "Services & Offerings",
        "title": "Software Development Services Scope & Discovery",
        "source": "Services Agreement",
        "url": "/Agreement",
        "text": """RC DEV LLC Services & Discovery Consultation:
RC DEV LLC (Contractor: Rodney Chery) provides custom software development, web engineering, cloud infrastructure, SRE support, and digital optimization.
- Complimentary Discovery Consultation: Clients receive up to one complimentary 30-minute discovery consultation to discuss business requirements, budget, timeline, and architectural feasibility. The discovery consultation does not include custom coding, database design, wireframes, or architecture blueprints.
- Scoped Deliverables: All work is performed under an approved Project Scope or Statement of Work. Any additions outside approved scope are handled via written Change Requests."""
    },
    {
        "id": "services-agreement-pricing-terms",
        "category": "Services & Offerings",
        "title": "Pricing Terms, 50% Deposit & Milestone Payments",
        "source": "Services Agreement",
        "url": "/Agreement",
        "text": """Project Pricing, Deposit Requirements, and Payment Milestones:
- 50% Non-Refundable Deposit: A 50% initial deposit of the total implementation fee is required before project scheduling, engineering, or design begins. The deposit compensates for scheduling allocation, administrative planning, and preliminary architectural execution.
- Final Payment & Milestone Billing: Remaining balance is invoiced upon project milestones or final acceptance prior to production deployment.
- Client Code Ownership: Upon payment in full, the client receives full ownership and intellectual property rights to custom software deliverables. Contractor retains standard reusable background components and developer toolchains."""
    },

    # ── PROJECTS & CASE STUDIES ──
    {
        "id": "project-dealerspike-integrations",
        "category": "Technical Projects",
        "title": "Dealer Spike Platform Integrations & Front-End Engineering",
        "source": "Portfolio Projects",
        "url": "/Projects",
        "text": """Dealer Spike Platform Customizations & Engineering:
Specialized front-end engineering for high-volume automotive, marine, and power-sports digital dealerships:
- Engineered custom JavaScript/jQuery modules for vehicle search filters, payment quote calculators, and dynamic SRP/VDP inventory displays.
- Integrated third-party analytics and marketing tracking (Google Tag Manager, Google Analytics 4, Tealium, Google Ads conversion beacons) with zero DOM layout shift.
- Optimized responsive layouts and resolved complex CSS/LESS cascading conflicts across shared multi-dealer template systems."""
    },
    {
        "id": "project-cybernetic-hud",
        "category": "Technical Projects",
        "title": "Cybernetic Portfolio & Edge Architecture (RC DEV)",
        "source": "Portfolio Projects",
        "url": "/Projects",
        "text": """RC DEV Cybernetic Portfolio & Cloudflare Edge Stack:
The portfolio itself is built as a cybernetic HUD web platform:
- Interactive CLI Shell (RC-SHELL): Live edge-based command drawer with AI natural language assistant connected to gpt-4o-mini and vector RAG.
- High-Performance Matrix Rain: 60 FPS HTML5 Canvas animation with automatic suppression in Reading Mode.
- Paper Reading Mode: High-contrast color inversion for deep article readability with persistent localStorage synchronization.
- Edge Architecture: Cloudflare Workers with asset binding, token bucket rate limiting on quote/support forms, and Cloudflare Turnstile bot protection."""
    },

    # ── ENGINEERING FAQS & STANDARDS ──
    {
        "id": "faq-performance-standards",
        "category": "Engineering Standards",
        "title": "Web Performance, Lighthouse Scores & Architecture Standards",
        "source": "Engineering FAQ",
        "url": "/Faq",
        "text": """Engineering Quality Standards & Performance Principles:
- 100/100 Lighthouse Target: Every project is optimized for maximum Core Web Vitals (LCP, FID/INP, CLS) through asset preloading, WebP/AVIF modern image formats, and zero blocking third-party scripts.
- Clean Architecture: Adherence to single-responsibility interfaces, strict input validation, and defensive programming.
- Security-First: Implementation of Content Security Policies (CSP), HTTP Strict Transport Security (HSTS), rate limiting, and encrypted environment secrets."""
    }
]


def get_openai_key():
    # Read from .dev.vars or environment
    if os.path.exists(".dev.vars"):
        with open(".dev.vars", "r") as f:
            for line in f:
                if line.startswith("OPENAI_API_KEY="):
                    return line.strip().split("=", 1)[1].strip()
    return os.environ.get("OPENAI_API_KEY", "").strip()


def embed_texts(texts, api_key):
    url = "https://api.openai.com/v1/embeddings"
    headers = {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {api_key}"
    }
    payload = {
        "model": "text-embedding-3-small",
        "input": texts,
        "dimensions": 512
    }
    req = urllib.request.Request(url, data=json.dumps(payload).encode("utf-8"), headers=headers, method="POST")
    with urllib.request.urlopen(req) as resp:
        res_data = json.loads(resp.read().decode("utf-8"))
        return [item["embedding"] for item in res_data["data"]]


def main():
    api_key = get_openai_key()
    if not api_key:
        print("ERROR: OPENAI_API_KEY not found in .dev.vars or environment.")
        return

    print(f"Building Vector RAG Knowledge Base with {len(CHUNKS)} multi-document chunks...")
    texts_to_embed = [c["text"] for c in CHUNKS]
    
    print("Generating 512-dimension embeddings via OpenAI text-embedding-3-small...")
    embeddings = embed_texts(texts_to_embed, api_key)

    for i, chunk in enumerate(CHUNKS):
        chunk["embedding"] = embeddings[i]

    output_path = os.path.join("worker", "rag-index.json")
    with open(output_path, "w", encoding="utf-8") as f:
        json.dump({
            "version": "1.0.0",
            "model": "text-embedding-3-small",
            "dimensions": 512,
            "totalChunks": len(CHUNKS),
            "chunks": CHUNKS
        }, f, indent=2)

    size_kb = os.path.getsize(output_path) / 1024
    print(f"SUCCESS: Generated Vector RAG store with {len(CHUNKS)} embedded chunks at {output_path} ({size_kb:.1f} KB).")


if __name__ == "__main__":
    main()
