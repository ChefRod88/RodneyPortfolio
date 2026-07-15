# Rodney Portfolio

**Live site:** [rodneyachery.com](https://www.rodneyachery.com)

I built this portfolio to showcase my background, skills, and projects. It includes a client portal for invoice viewing and payments.

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Technology Stack](#2-technology-stack)
3. [Architecture Overview](#3-architecture-overview)
4. [Project Structure](#4-project-structure)
5. [Features](#5-features)
6. [API Endpoints](#6-api-endpoints)
7. [Configuration](#7-configuration)
8. [Recent Changes](#8-recent-changes)
9. [Documentation](#9-documentation)

---

## 1. Executive Summary

This is a single-page portfolio built with **ASP.NET Core 10**, **Razor Pages**, **C#**, and **JavaScript**. It is hosted as a static site on **Cloudflare Pages** using **GitHub Actions** CI/CD.

**Sections:**
- **Profile/Hero** — Introduction, status badge, skills row, social links, CV download
- **About** — Education, photo collage, career narrative
- **Experience** — Six skill cards (Soft Skills, IT Support, Frontend, Backend, Tools, AI/LLM)
- **Projects** — Project catalog, GitHub repo
- **FAQ** — Frequently asked questions
- **Contact** — Email, LinkedIn

---

## 2. Technology Stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 10.0 |
| Backend | ASP.NET Core (Razor Pages + MVC) |
| Frontend | HTML5, CSS3, JavaScript, Bootstrap |
| Payments | Stripe |
| Analytics | Google Analytics 4 |
| Hosting | Cloudflare Pages |
| CI/CD | GitHub Actions |

---

## 3. Architecture Overview

```
User Browser → /Portal → PortalController (MVC)
                                    ↓
                    IAccountService / IOtpService / ISessionService
                                    ↓
                    IInvoiceService + Stripe Payment
```

**Flow:** The client portal uses OTP email verification, session-based auth, and Stripe for payments.

---

## 4. Project Structure

```
RodneyPortfolio/
├── Controllers/
│   ├── ChatController.cs          # POST /api/chat, /api/chat/job-match
│   └── PortalController.cs        # GET/POST /Portal (login, register, verify, dashboard)
├── Services/
│   ├── IAccountService.cs         # Client account CRUD interface
│   ├── IOtpService.cs             # OTP generation/validation interface
│   ├── ISessionService.cs         # Session create/get/invalidate interface
│   ├── IQuoteEmailService.cs      # Quote email sending interface
│   ├── IQuoteLogService.cs        # Quote logging interface
│   ├── QuoteEmailService.cs       # Quote email implementation
│   ├── QuoteLogService.cs         # Quote log implementation
│   ├── QuoteSanitizer.cs          # Quote input sanitization
│   ├── SqlClientPortalService.cs  # SQL-backed portal service
│   ├── SqlInvoiceService.cs       # SQL-backed invoice service
│   └── ...                        # OpenAIChatService, JobMatchService, etc.
├── ViewModels/
│   ├── PortalLoginViewModel.cs
│   ├── PortalRegisterViewModel.cs
│   ├── PortalVerifyViewModel.cs
│   └── PortalDashboardViewModel.cs
├── Views/Portal/                  # MVC views for portal
│   ├── Index.cshtml
│   ├── Login.cshtml
│   ├── Register.cshtml
│   ├── Verify.cshtml
│   └── Dashboard.cshtml
├── Models/                        # ChatRequest, ChatResponse, Invoice, ClientAccount, etc.
├── Data/ResumeContext.txt         # Resume + about content for AI
├── Pages/Index.cshtml             # Main portfolio page
├── Pages/Admin/                   # Admin pages (Accounts, Invoices, EditClient)
├── wwwroot/                       # CSS, JS, assets, PWA
├── docs/                          # Technical documentation
└── .github/workflows/             # CI/CD to Cloudflare
```

---

## 5. Features

- **Client Portal** — OTP email verification login, client dashboard, invoice history, Stripe payments.
- **Admin Panel** — Manage client accounts, invoices, and edit client details.
- **Quote Submission** — Contact/quote form with input sanitization, email delivery, and logging.
- **PWA** — Service worker for offline caching.
- **Input validation** — Max length, prompt injection blocking.
- **Content filtering** — Profanity block list.

---

## 6. API Endpoints

| Endpoint | Method | Request | Response |
|----------|--------|---------|----------|
| `/Portal` | GET | — | Portal landing page |
| `/Portal/Register` | GET/POST | Email, name | OTP verification flow |
| `/Portal/Login` | GET/POST | Email | OTP verification flow |
| `/Portal/Verify` | GET/POST | OTP code | Session cookie |
| `/Portal/Dashboard` | GET | — | Invoices + payment links |

---

## 7. Configuration

- **Production:** GitHub Secret `CLOUDFLARE_API_TOKEN` (Wrangler deployment token)
- **GA4:** `GoogleAnalytics:MeasurementId` in appsettings or `GA4_MEASUREMENT_ID` GitHub Secret
- **Stripe:** `Stripe:SecretKey` and `Stripe:PublishableKey` in appsettings/secrets
- **reCAPTCHA v2 local setup:**
  - `dotnet user-secrets set "Recaptcha:SiteKey" "<your-site-key>"`
  - `dotnet user-secrets set "Recaptcha:SecretKey" "<your-secret-key>"`
  - `dotnet user-secrets set "Recaptcha:ExpectedHostname" "localhost"`
- **reCAPTCHA v2 production setup:** Configure values for `Recaptcha:SiteKey`, `Recaptcha:SecretKey`, and `Recaptcha:ExpectedHostname` (for this site: `www.rodneyachery.com`).

---

## 8. Recent Changes

### Portal Refactored to MVC
- Removed Razor Pages: `Login.cshtml`, `Register.cshtml`, `Portal.cshtml`, `Dashboard.cshtml`, `Verify.cshtml`
- Replaced with a single `PortalController.cs` (MVC) and `Views/Portal/` views
- Added `ViewModels/` folder with `PortalLoginViewModel`, `PortalRegisterViewModel`, `PortalVerifyViewModel`, `PortalDashboardViewModel`

### New Services
- `IAccountService` / `IOtpService` / `ISessionService` — clean interfaces for account management, OTP auth, and session handling
- `IQuoteEmailService` / `QuoteEmailService` — dedicated quote email sending
- `IQuoteLogService` / `QuoteLogService` — quote submission logging
- `QuoteSanitizer` — input sanitization for quote submissions
- `IClientPortalService` removed; functionality split into focused interfaces

### Hero Section Updates
- Added status badge and skills row (C#, .NET, Azure, MS SQL/SSMS, etc.) under the hero tagline
- Moved social links under the hero tagline
- Removed "Get a Quote" button from hero

### Styles
- Expanded `wwwroot/css/site.css` with portal, admin, and hero styles

---

## 9. Documentation

| Document | Description |
|----------|-------------|
| [Complete Technical Documentation](docs/RODNEY_PORTFOLIO_TECHNICAL_DOCUMENTATION.md) | Full A–Z breakdown: HTML, CSS, JS, C#, APIs, architecture |
| [Codebase Explained](docs/CODEBASE_EXPLAINED_SIMPLE.md) | Plain-English explanation for recruiters |

---

**Author:** Rodney Chery
**Repository:** [github.com/ChefRod88/RodneyPortfolio](https://github.com/ChefRod88/RodneyPortfolio)
