#!/usr/bin/env python3
"""
Build Complete Codebase & Multi-Document Vector RAG Knowledge Base for Rodney Chery Portfolio.
Generates 512-dimensional vector embeddings using OpenAI text-embedding-3-small
across Resume, Technical Articles, Services & Compliance, Projects, FAQs, and Authored Codebase.
Reuses existing embeddings when available to optimize token usage.
Saves the compiled vector store to worker/rag-index.json.
"""

import os
import json
import urllib.request
import urllib.error

# ── 1. AUTHORITATIVE KNOWLEDGE BASE CHUNKS (Resume, Articles, Terms, Projects) ──
CORE_CHUNKS = [
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
        "text": """Freelance Full-Stack Developer (Feb 2023 – Present, Winter Haven, FL):
- Built dynamic web interfaces from scratch using HTML, CSS, JavaScript — debounced search, modal forms, event delegation, and dynamic table rendering.
- Built RESTful and OData v4 APIs using C# and ASP.NET Core, deployed with automated CI/CD pipelines.
- Designed relational SQL Server and SQLite databases with stored procedures, foreign key relationships, and aggregate queries.

Minister of Technology at New Bethel Missionary Baptist Church (2025 – Present, Winter Haven, FL):
- Built and actively maintains a live production web application for the congregation — HTML, CSS, JavaScript, event management, and YouTube livestream integration.
- Translates non-technical requirements from leadership into working production features."""
    },
    {
        "id": "resume-skills-stack",
        "category": "Technical Skills",
        "title": "Comprehensive Technical Skillset & Stack",
        "source": "Official Resume",
        "url": "/#services",
        "text": """Rodney Chery's Technical Skillset & Stack:
- Core Languages & Web: JavaScript (ES6+), jQuery, HTML5, CSS3/LESS, Bootstrap, DOM APIs, MutationObserver, Classic ASP, C#, .NET MAUI, REST APIs, JSON, SQL, SQLite.
- Cloud & DevOps: Cloudflare, AWS, Git, GitHub, Dealer Spike CMS, FTP/SFTP, VS Code, Windows OS.
- Analytics & Technical SEO: Chrome DevTools, JSON-LD, Schema.org Structured Data, Google Tag Manager (GTM), Google Analytics 4 (GA4), Tealium, Technical SEO, canonical URLs, crawler directives, HTTP status codes.
- Enterprise Support & Systems: Salesforce Service Cloud, Jira, Active Directory, Wireshark, SLA Incident Resolution, Client Communications."""
    },
    {
        "id": "resume-education-certs",
        "category": "Education & Certifications",
        "title": "Certifications & Education Credentials",
        "source": "Official Resume",
        "url": "/#about",
        "text": """Certifications & Formal Education:
- AWS Certified Cloud Practitioner
- ITIL 4 Foundation
- Google IT Support Professional Certificate
- B.S. in Software Engineering — Western Governors University (WGU, Expected Completion: December 2026)."""
    },
    {
        "id": "article-ai-support-workflow-arch",
        "category": "Published Engineering Articles",
        "title": "AI-Assisted Support Engineering: Architecture & Dual-Plane Separation",
        "source": "Article: AI-Assisted Support Engineering Workflow",
        "url": "/Articles/ai-assisted-support-engineering-workflow",
        "text": """From the published article 'How I Built an AI-Assisted Support Engineering Workflow' by Rodney Chery:
Managing live client websites in high-volume SLA support requires speed without compromising production safety.
The Dual-Plane Separation Architecture:
1. Operational Plane: The physical environment containing the VPN tunnel, staging and production servers, and SFTP transfer pipes. AI models have ZERO access to this plane. No API keys, no tools, no automated pushes.
2. Contextual Plane: An isolated local workspace where read-only sanitized site copies, ticket context, and technical assets reside. This is where AI agents operate under Interpretable Context Methodology (ICM).
This separation guarantees that even in the event of hallucination, an agent can never modify a live production site."""
    },
    {
        "id": "article-ai-support-workflow-automation",
        "category": "Published Engineering Articles",
        "title": "PowerShell & WinSCP Automation in Support Engineering",
        "source": "Article: AI-Assisted Support Engineering Workflow",
        "url": "/Articles/ai-assisted-support-engineering-workflow",
        "text": """From 'How I Built an AI-Assisted Support Engineering Workflow' by Rodney Chery:
Automation Details:
- Uses separate PowerShell scripts: Get-LiveSite.ps1 and Get-StageSite.ps1 to make accidental cross-environment execution physically impossible.
- Automates SFTP transfers via the WinSCP .NET assembly (WinSCPnet.dll) with pinned SSH host key fingerprints.
- Creates immutable local baselines before touching code.
- Scripted diff checks ensure only intended changes are deployed.
- Rollback scripts (Restore-Baseline.ps1) allow instant recovery if unexpected behavior occurs."""
    },
    {
        "id": "article-ai-support-workflow-icm",
        "category": "Published Engineering Articles",
        "title": "Interpretable Context Methodology (ICM) Agentic Stages",
        "source": "Article: AI-Assisted Support Engineering Workflow",
        "url": "/Articles/ai-assisted-support-engineering-workflow",
        "text": """From 'How I Built an AI-Assisted Support Engineering Workflow' by Rodney Chery:
The Interpretable Context Methodology (ICM) executes tasks in sequential, auditable stages:
Stage 0 (Identity): Sets strict operational rules, persona constraints, and security bounds.
Stage 1 (Skill Extraction): Analyzes user request, parses tickets, extracts core technical requirements.
Stage 2 (Resume Comparison / Diagnostic Diff): Evaluates changes against baselines.
Stage 3 (Implementation / Output Generation): Produces clean, targeted code solutions with full explanation.
Every stage produces inspectable markdown logs in IcmWorkspace/, ensuring complete human auditability."""
    },
    {
        "id": "article-zerocost-aspnetcore-cloudflare",
        "category": "Published Engineering Articles",
        "title": "Zero-Cost ASP.NET Core: Static Site Generation on Cloudflare Pages",
        "source": "Article: Zero-Cost ASP.NET Core",
        "url": "/Articles/zero-dollar-aspnetcore-cloudflare-ssg",
        "text": """From 'Zero-Cost ASP.NET Core: Building a Markdown Blog on Cloudflare Pages' by Rodney Chery:
Traditional ASP.NET Core hosting requires dedicated App Service plans or VPS instances that incur monthly cloud costs.
Rodney engineered a zero-cost architecture:
1. ASP.NET Core handles Markdown parsing (Markdig), frontmatter extraction, syntax highlighting, and Razor page layouts.
2. A local crawler / static site generator pipeline runs at build time, scraping rendered pages into pure HTML, CSS, and JS in wwwroot/.
3. The compiled static bundle is deployed to Cloudflare Pages and Workers for global edge delivery with 0ms cold starts and 0 cloud hosting cost.
4. Preserves SEO, OpenGraph tags, RSS feeds, and XML sitemaps."""
    },
    {
        "id": "agreement-services-scope-discovery",
        "category": "Client Services & Contracts",
        "title": "Software Development Services Scope & Discovery",
        "source": "Services Agreement",
        "url": "/Agreement",
        "text": """Rodney Chery Services Agreement — Scope & Discovery:
- Consulting and development services include web application development, API integration, performance optimization, troubleshooting, and cloud edge hosting.
- Discovery Consultations: The initial 30-minute discovery consultation is strictly limited to high-level architectural assessment, timeline feasibility, and project scope alignment.
- Discovery meetings do NOT include direct hands-on code authoring, production debugging, or deployment execution prior to a formal signed agreement and deposit."""
    },
    {
        "id": "agreement-pricing-deposit-milestones",
        "category": "Client Services & Contracts",
        "title": "Pricing Terms, 50% Deposit & Milestone Payments",
        "source": "Services Agreement",
        "url": "/Agreement",
        "text": """Rodney Chery Services Agreement — Pricing & Payment Terms:
- Deposit: All freelance and custom development projects require a 50% non-refundable deposit of the total estimated implementation fee before scheduling, engineering, or design commences.
- Milestones: The remaining balance is invoiced upon milestone completion or final acceptance prior to production deployment.
- Invoicing: Payments are accepted via Stripe, CashApp Business, or direct bank transfer.
- Late Payments: Past-due balances accrue interest at 1.5% per month or the statutory maximum."""
    },
    {
        "id": "faq-engineering-standards-lighthouse",
        "category": "Engineering Standards",
        "title": "Web Performance, Lighthouse Scores & Architecture Standards",
        "source": "Engineering FAQ",
        "url": "/Faq",
        "text": """Rodney Chery Portfolio Engineering Standards:
- Performance: Achieves 100/100 Google Lighthouse scores across Performance, Accessibility, Best Practices, and SEO.
- Zero-Latency Edge Delivery: Hosted on Cloudflare's global edge network across 300+ data centers.
- Security: Uses Content Security Policy (CSP), Cloudflare Turnstile bot verification, strict CORS headers, and rate-limiting middleware (5 requests/60s).
- Accessibility: Fully keyboard-navigable, high-contrast accessible color palette, reading mode toggle, and ARIA compliance."""
    },
    {
        "id": "projects-portfolio-interactive-hud",
        "category": "Featured Projects",
        "title": "Cybernetic Portfolio HUD & RC-SHELL Terminal System",
        "source": "Portfolio Architecture",
        "url": "/Projects",
        "text": """Rodney Chery Portfolio Featured Projects:
1. Cybernetic Developer HUD: A high-performance portfolio featuring an integrated interactive CLI terminal (RC-SHELL), dynamic theme inversion, and neural AI assistant.
2. Dealer Spike Production Widgets: Custom JavaScript DOM widgets for inventory search, dynamic pricing calculations, and payment estimators in live automotive dealership websites.
3. C# .NET Core Clean Architecture APIs: Production RESTful and OData services deployed with automated GitHub Actions CI/CD."""
    }
]

# ── 2. CODEBASE SOURCE CHUNKS (C#, Worker, Frontend, Scripts, CI/CD) ──
CODEBASE_CHUNKS = [
    {
        "id": "code-worker-routing-and-security",
        "category": "Cloudflare Edge Architecture",
        "title": "Cloudflare Edge Worker: Dynamic Routing, Turnstile & Rate Limiter Middleware",
        "source": "worker/index.js",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/worker/index.js",
        "text": """Cloudflare Worker Core Architecture (worker/index.js):
Handles dynamic routing, rate limiting, and security for the portfolio on Cloudflare's global edge.
- Routing: Directs `POST /?handler=Quote` to `handleQuote`, `POST /Support` to `handleSupport`, and `POST /api/chat` to `handleChat`. All other requests fall through to static `env.ASSETS`.
- Rate Limiting: Uses Cloudflare bindings (`QUOTE_RATE_LIMITER` and `SUPPORT_RATE_LIMITER`) to enforce a 5 requests/60s rate limit per IP using `limiter.limit({ key: ip })`.
- Input Sanitization: `sanitize(value)` strips control characters and newlines (`/[\\r\\n\\0\\x01-\\x1f]/g`) to neutralize log and header injection vulnerabilities.
- Bot Protection: `verifyTurnstile(env, token, ip)` validates Cloudflare Turnstile tokens against `challenges.cloudflare.com/turnstile/v0/siteverify`.
- Email Notifications: `sendEmail(env, { subject, text, replyTo })` dispatches verified inquiries through the Resend API."""
    },
    {
        "id": "code-worker-vector-rag-handler",
        "category": "Cloudflare Edge Architecture",
        "title": "Edge Multi-Document Vector RAG Chat Handler & OpenAI Proxy",
        "source": "worker/index.js",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/worker/index.js",
        "text": """Edge Vector RAG Chat Implementation (worker/index.js -> handleChat):
Routes terminal queries through OpenAI embeddings and GPT-4o-mini with real-time vector document retrieval.
- Query Validation: Sanitizes input and limits length to 500 characters.
- Vector Retrieval: Calls `getQueryEmbedding(userMessage, apiKey)` to generate a 512-dim query embedding, then `retrieveRelevantChunks(queryVector, 3, 0.32)` to find top-scoring chunks from rag-index.json.
- Prompt Synthesis: Formats retrieved chunks with document titles, source categories, and URLs into the system prompt.
- Response Formatting: Instructs GPT-4o-mini to speak as Rodney's assistant and cite markdown links for articles or agreements. Returns `{ ok: true, reply, sources: [...] }`.
- Graceful Fallback: If OpenAI embeddings fail or time out, gracefully falls back to the baseline resume context without throwing an error."""
    },
    {
        "id": "code-worker-rag-math-engine",
        "category": "Vector Search & Algorithms",
        "title": "Vector RAG Engine: L2 Cosine Dot Product, Top-K Retrieval & Context Formatting",
        "source": "worker/rag.js",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/worker/rag.js",
        "text": """Edge Vector Similarity & Retrieval Engine (worker/rag.js):
Implements fast, dependency-free vector cosine similarity directly in Cloudflare V8 runtime.
- `cosineSimilarity(vecA, vecB)`: Because embeddings from OpenAI text-embedding-3-small are unit-normalized (L2 norm = 1.0), cosine similarity is computed as the pure dot product sum(A[i] * B[i]) in O(N) time (< 0.2ms for 512 dimensions).
- `getQueryEmbedding(query, apiKey)`: Calls OpenAI `/v1/embeddings` requesting model 'text-embedding-3-small' with 'dimensions: 512'.
- `retrieveRelevantChunks(queryVector, topK = 3, minScore = 0.35)`: Computes similarity across all embedded chunks, sorts descending by score, filters by minScore threshold, and returns top K matches.
- `formatRagContext(chunks)`: Structures retrieved chunks into markdown blocks with document headers, URLs, and similarity scores for LLM prompt injection."""
    },
    {
        "id": "code-frontend-terminal-shell",
        "category": "Frontend UI & Terminal",
        "title": "RC-SHELL Interactive Terminal: Command Registry, Markdown Link Renderer & AI Uplink",
        "source": "wwwroot/js/site.js",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/wwwroot/js/site.js",
        "text": """RC-SHELL Terminal Emulator (wwwroot/js/site.js):
A cybernetic slide-out terminal emulator providing instant CLI navigation and neural AI query modes.
- Commands Dictionary: Built-in handlers for 'help', 'about', 'skills', 'experience', 'articles', 'projects', 'contact', 'clear', and 'status'.
- AI Question Dispatch: Any non-command query is sent to `POST /api/chat` with a dynamic loading spinner ('RC-AI // Vector RAG search & neural synthesis...').
- `renderTerminalMarkdown(str)`: Escapes raw HTML to prevent XSS, then transforms markdown hyperlinks `[text](url)` into styled clickable anchors (`style='color:var(--c);font-weight:600;'`) and `**text**` into strong tags.
- Drawer Mechanics: Smooth CSS transition slide-out drawer with keyboard focus handling and backdrop toggling."""
    },
    {
        "id": "code-frontend-reading-mode-toggle",
        "category": "Frontend UI & Accessibility",
        "title": "Reading Mode Inverter: Dark/Light Contrast Toggle & Accessible High-Readability DOM State",
        "source": "wwwroot/js/site.js",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/wwwroot/js/site.js",
        "text": """Reading Mode & Theme Inversion (wwwroot/js/site.js):
Controls the accessible reading mode toggle (#themeToggleBtn) for enhanced contrast and readability.
- State Management: Listens for clicks on #themeToggleBtn and toggles the '.reading-mode' class on `document.documentElement`.
- Persistence: Saves preference in `localStorage.setItem('rc-reading-mode', ...)` and restores state on DOMContentLoaded.
- CSS Inversion: Dynamically shifts CSS variables from cybernetic dark palette to high-contrast paper/light tokens, adjusting typography contrast and disabling distracting scanlines while maintaining 100/100 Lighthouse accessibility."""
    },
    {
        "id": "code-frontend-job-match-analyzer",
        "category": "Frontend AI Features",
        "title": "Interactive Job Match Analyzer: Resume ATS Keyword Comparison & Fit Scoring",
        "source": "wwwroot/js/site.js",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/wwwroot/js/site.js",
        "text": """Job Match Feature (wwwroot/js/site.js):
Enables recruiters and hiring managers to paste a job description and receive a real-time skills comparison against Rodney's resume.
- Interaction: Listens on `#jobMatchForm`, extracts job description text, and enforces character limits.
- API Dispatch: Posts to `/api/chat/job-match` with user input.
- Result Rendering: Displays matched core technologies, percentage alignment score, and tailored narrative analysis detailing how Rodney's Tier III LeadVenture and full-stack experience aligns with the specific role."""
    },
    {
        "id": "code-frontend-article-reader",
        "category": "Frontend UI & Reading Experience",
        "title": "Technical Article Reader: Scroll Progress Indicator & Dynamic Table of Contents",
        "source": "wwwroot/js/article-reader.js",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/wwwroot/js/article-reader.js",
        "text": """Article Reader Experience (wwwroot/js/article-reader.js):
Enhances long-form technical article readability across desktop and mobile.
- Reading Progress: Calculates viewport scroll percentage relative to article height and animates the top progress bar (#articleProgressBar).
- Dynamic Table of Contents: Scans article markup for `<h2>` and `<h3>` tags, generates semantic slug anchors, and builds an interactive sticky navigation sidebar.
- Reader Controls: Provides font sizing buttons (standard, large, x-large) with user preference stored in localStorage."""
    },
    {
        "id": "code-frontend-projects-filter",
        "category": "Frontend UI & Portfolio Display",
        "title": "Projects Directory: Stack Tag Filtering, Search & Modal Architecture",
        "source": "wwwroot/js/projects-page.js",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/wwwroot/js/projects-page.js",
        "text": """Projects Page Interactive Filter (wwwroot/js/projects-page.js):
Powers the multi-category technical project gallery.
- Tag-Based Filtering: Allows filtering by tech tags including 'C# / .NET', 'Cloudflare', 'JavaScript / DOM', 'Classic ASP', and 'Automation'.
- Deep Linking: Updates URL query strings (`?tech=csharp`) so specific project views can be bookmarked or shared with hiring managers.
- Modal Inspection: Expands project cards into detailed architecture views with architecture diagrams, key features, and GitHub repository links."""
    },
    {
        "id": "code-frontend-invoice-payment-modal",
        "category": "Frontend UI & Client Workflow",
        "title": "Client Invoice Payment Modal: Scroll Lock & CashApp / Stripe Checkout Integration",
        "source": "wwwroot/js/invoice-payment-modal.js",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/wwwroot/js/invoice-payment-modal.js",
        "text": """Invoice Payment Modal (wwwroot/js/invoice-payment-modal.js):
Manages freelance invoice payments and client checkout workflows.
- Modal Mechanics: Handles opening/closing of the invoice payment modal, trapping focus for accessibility and locking body scrolling (`overflow: hidden`) to prevent background jump.
- Payment Options: Displays CashApp QR code and direct Stripe checkout links for milestone and 50% deposit payments.
- Keyboard Support: Listens for Escape key presses to dismiss modal overlays gracefully."""
    },
    {
        "id": "code-frontend-pwa-service-worker",
        "category": "PWA & Performance",
        "title": "Progressive Web App Service Worker: Cache-First Offline Strategy & Cache Invalidation",
        "source": "wwwroot/sw.js",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/wwwroot/sw.js",
        "text": """PWA Service Worker (wwwroot/sw.js):
Provides offline resiliency, asset caching, and near-zero load times.
- Cache Strategy: Uses cache-first strategy for static assets (CSS, JS, fonts, images) and network-first strategy for dynamic `/api/*` requests.
- Cache Name: Versioned cache namespace (`rc-portfolio-v3`) that automatically deletes old caches on new deployment activation.
- Pre-caching: Pre-caches critical routes: `/`, `/Projects`, `/Articles`, `/Faq`, `/Agreement`, `/Support`, `/css/site.css`, `/js/site.js`, and `/manifest.json`."""
    },
    {
        "id": "code-csharp-content-filter-tests",
        "category": "C# Backend & Security Testing",
        "title": "C# ContentFilter Unit Tests: Inappropriate Content Filtering & Boundary Word Handling",
        "source": "tests/ContentFilterTests.cs",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/tests/ContentFilterTests.cs",
        "text": """C# ContentFilter Unit Tests (tests/ContentFilterTests.cs):
Tests the defensive content filtering service (RodneyPortfolio.Services.IContentFilter).
- Null Safety: Verifies that null or empty input safely returns false without throwing exceptions.
- Clean Input: Asserts that valid professional inquiries ('What is Rodney's experience?', 'Tell me about his skills') are accepted.
- Boundary Word Protection: Verifies regex word boundary matching so harmless words containing banned substrings (e.g. 'class', 'pass') are NOT falsely blocked.
- Blocked Term Neutralization: Confirms that profane or abusive submissions are rejected immediately."""
    },
    {
        "id": "code-csharp-input-validator-tests",
        "category": "C# Backend & Quality Assurance",
        "title": "C# InputValidator Unit Tests: RFC Email Validation, Bounds Checking & Sanitization",
        "source": "tests/InputValidatorTests.cs",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/tests/InputValidatorTests.cs",
        "text": """C# InputValidator Unit Tests (tests/InputValidatorTests.cs):
Validates user submission sanitization and format verification (IInputValidator).
- Email Validation: Tests RFC-compliant email verification, rejecting malformed emails and accepting standard business formats.
- Length Constraints: Enforces maximum string lengths on contact fields (Name: 100 chars, Message: 2000 chars) to prevent buffer and memory exhaustion attacks.
- Cross-Site Scripting (XSS) Prevention: Ensures HTML tags and script injections are neutralized before reaching storage or notification dispatchers."""
    },
    {
        "id": "code-csharp-turnstile-tests",
        "category": "C# Backend & Bot Defense",
        "title": "C# Turnstile Verification Service Tests: Cloudflare Captcha Token Validation",
        "source": "tests/TurnstileVerificationServiceTests.cs",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/tests/TurnstileVerificationServiceTests.cs",
        "text": """C# Turnstile Verification Service Tests (tests/TurnstileVerificationServiceTests.cs):
Tests server-side token validation for Cloudflare Turnstile bot protection.
- Mocking: Uses Moq and HttpMessageHandler to mock responses from challenges.cloudflare.com.
- Success Path: Asserts that valid tokens with matching secret keys return `{ success: true }`.
- Failure Path: Verifies that expired tokens or bot challenges trigger proper error handling (`invalid-input-response`, `timeout-or-duplicate`) and reject malicious requests."""
    },
    {
        "id": "code-csharp-dual-ai-chat-tests",
        "category": "C# Backend & AI Architecture",
        "title": "C# Dual AI Service Tests: Multi-Provider LLM Orchestration & Failover Mechanics",
        "source": "tests/DualAIChatServiceTests.cs",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/tests/DualAIChatServiceTests.cs",
        "text": """C# Dual AI Chat Service Tests (tests/DualAIChatServiceTests.cs):
Tests multi-provider LLM orchestration and automatic failover patterns.
- Multi-Provider Support: Interfaces with both OpenAI and Anthropic Claude clients.
- Resilient Failover: Verifies that if the primary LLM provider times out, returns HTTP 500, or encounters rate limits, the system seamlessly redirects the prompt to the secondary provider.
- Token Optimization: Asserts prompt trimming and token budgets are enforced to keep response latency low."""
    },
    {
        "id": "code-csharp-button-contract-tests",
        "category": "C# Quality Assurance & Contract Testing",
        "title": "C# Button Wiring Contract Tests: Static HTML Element ID & Click Binding Validation",
        "source": "tests/ButtonWiringContractTests.cs",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/tests/ButtonWiringContractTests.cs",
        "text": """C# Button Wiring Contract Tests (tests/ButtonWiringContractTests.cs):
Automated contract tests that protect against dead buttons and broken DOM bindings during static site generation.
- HTML Parsing: Loads compiled HTML pages from wwwroot/ and verifies that every interactive CTA, modal open button, and filter trigger contains an expected DOM ID.
- Script Verification: Asserts that client JavaScript files reference matching selector IDs so no interactive controls ever fail silently in production."""
    },
    {
        "id": "code-scripts-knowledge-graph-indexer",
        "category": "Developer Tooling & Knowledge Graph",
        "title": "Medical App Knowledge Graph Indexer: AST Symbol Extraction & Graph Topology Generator",
        "source": "scripts/index_codebase.py",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/scripts/index_codebase.py",
        "text": """Knowledge Graph Indexer (scripts/index_codebase.py):
Builds an AST symbol knowledge graph across C#, JavaScript, Python, and Markdown files.
- Symbol Extraction: Uses regex and AST patterns to discover classes, interfaces, methods, route handlers, and imports.
- Graph Topology: Generates a directed graph of code entities and dependency edges (imports, calls, implements).
- Output: Exports index.json containing nodes and relations to power the local MCP medical-app-knowledge-graph server."""
    },
    {
        "id": "code-scripts-icm-agentic-pipeline",
        "category": "AI Agent Architecture",
        "title": "Interpretable Context Methodology (ICM) Pipeline: 3-Stage Sequential Agent Runner",
        "source": "scripts/run_icm_pipeline.py",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/scripts/run_icm_pipeline.py",
        "text": """ICM Sequential Pipeline Runner (scripts/run_icm_pipeline.py):
Executes the multi-stage Interpretable Context Methodology agentic workflow.
- Stage 1: Skill Extraction (parses target job descriptions and extracts structured technical requirements).
- Stage 2: Resume Comparison (performs a gap analysis against Rodney's authoritative resume).
- Stage 3: Interview Preparation (generates contextual STAR behavioral and technical interview talking points).
- Audit Trail: Enforces immutability by writing stage outputs to separate markdown directories under IcmWorkspace/."""
    },
    {
        "id": "code-cicd-github-actions-workflow",
        "category": "DevOps & CI/CD",
        "title": "GitHub Actions CI/CD: Automated Vitest Validation & Zero-Downtime Cloudflare Deployment",
        "source": ".github/workflows/main_rodney-portfolio.yml",
        "url": "https://github.com/ChefRod88/RodneyPortfolio/blob/main/.github/workflows/main_rodney-portfolio.yml",
        "text": """GitHub Actions CI/CD Pipeline (.github/workflows/main_rodney-portfolio.yml):
Automates continuous integration, testing, and edge deployment on every push to main.
- Job 1 (build_and_test): Checks out repository, configures Node.js, installs dependencies, validates Cloudflare Worker bundle with `wrangler deploy --dry-run`, and runs unit tests via `npm run test:js` (Vitest).
- Job 2 (deploy): Triggers upon successful test completion, using `cloudflare/wrangler-action@v3` with Cloudflare API tokens to push updated static assets and worker code with zero downtime."""
    }
]

ALL_CHUNKS = CORE_CHUNKS + CODEBASE_CHUNKS

def get_api_key():
    key = os.environ.get("OPENAI_API_KEY", "").strip()
    if key:
        return key

    dev_vars_path = os.path.join(os.path.dirname(__file__), "..", ".dev.vars")
    if os.path.exists(dev_vars_path):
        with open(dev_vars_path, "r", encoding="utf-8") as f:
            for line in f:
                if line.startswith("OPENAI_API_KEY="):
                    return line.split("=", 1)[1].strip().strip('"').strip("'")
    return ""

def fetch_embedding(text, api_key):
    url = "https://api.openai.com/v1/embeddings"
    payload = json.dumps({
        "model": "text-embedding-3-small",
        "dimensions": 512,
        "input": text
    }).encode("utf-8")

    req = urllib.request.Request(
        url,
        data=payload,
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {api_key}"
        }
    )

    try:
        with urllib.request.urlopen(req, timeout=30) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            return data["data"][0]["embedding"]
    except urllib.error.HTTPError as e:
        error_body = e.read().decode("utf-8")
        raise RuntimeError(f"OpenAI API HTTP {e.code}: {error_body}")
    except Exception as e:
        raise RuntimeError(f"OpenAI API Request failed: {e}")

def main():
    api_key = get_api_key()
    if not api_key:
        print("ERROR: OPENAI_API_KEY not found in environment or .dev.vars")
        exit(1)

    out_path = os.path.join(os.path.dirname(__file__), "..", "worker", "rag-index.json")

    # Load existing embeddings if file exists
    cached_embeddings = {}
    if os.path.exists(out_path):
        try:
            with open(out_path, "r", encoding="utf-8") as f:
                old_data = json.load(f)
                for chunk in old_data.get("chunks", []):
                    if "id" in chunk and "embedding" in chunk:
                        cached_embeddings[chunk["id"]] = chunk["embedding"]
            print(f"Loaded {len(cached_embeddings)} cached embeddings from worker/rag-index.json")
        except Exception as e:
            print(f"Warning: could not load existing cache: {e}")

    print(f"Building Full Codebase & Multi-Document Vector RAG Knowledge Base ({len(ALL_CHUNKS)} chunks)...")

    results = []
    new_embeddings_count = 0
    cached_embeddings_count = 0

    for i, chunk in enumerate(ALL_CHUNKS):
        cid = chunk["id"]
        title = chunk["title"]

        if cid in cached_embeddings:
            print(f"[{i+1}/{len(ALL_CHUNKS)}] Reusing cached embedding for: {title}")
            embedding = cached_embeddings[cid]
            cached_embeddings_count += 1
        else:
            print(f"[{i+1}/{len(ALL_CHUNKS)}] Generating new 512-dim embedding for: {title}...")
            embed_input = f"{chunk['title']}\nCategory: {chunk['category']}\nSource: {chunk['source']}\n\n{chunk['text']}"
            embedding = fetch_embedding(embed_input, api_key)
            new_embeddings_count += 1

        results.append({
            "id": chunk["id"],
            "category": chunk["category"],
            "title": chunk["title"],
            "source": chunk["source"],
            "url": chunk["url"],
            "text": chunk["text"],
            "embedding": embedding
        })

    index_data = {
        "version": "2.0.0",
        "model": "text-embedding-3-small",
        "dimensions": 512,
        "totalChunks": len(results),
        "chunks": results
    }

    with open(out_path, "w", encoding="utf-8") as f:
        json.dump(index_data, f, indent=2)

    size_kb = os.path.getsize(out_path) / 1024
    print(f"\nSUCCESS: Generated Full Codebase Vector RAG store with {len(results)} chunks.")
    print(f" - Reused cached: {cached_embeddings_count}")
    print(f" - Newly embedded: {new_embeddings_count}")
    print(f" - Output file: worker/rag-index.json ({size_kb:.1f} KB)")

if __name__ == "__main__":
    main()
