# Rodney Portfolio

**Live site:** [rodneyachery.com](https://www.rodneyachery.com)

I built this portfolio to showcase my background, skills, and projects. It includes an AI chatbot that answers questions about my resume and a Job Match feature that analyzes compatibility with pasted job descriptions.

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Technology Stack](#2-technology-stack)
3. [Architecture Overview](#3-architecture-overview)
4. [Project Structure](#4-project-structure)
5. [Features](#5-features)
6. [API Endpoints](#6-api-endpoints)
7. [Configuration](#7-configuration)
8. [Documentation](#8-documentation)

---

## 1. Executive Summary

This is a single-page portfolio built with **ASP.NET Core 10**, **Razor Pages**, **C#**, **JavaScript**, and **OpenAI Chat Completions API**. I host it on **Azure Web App** with **GitHub Actions** CI/CD.

**Sections:**
- **Profile/Hero** — Introduction, CV download, social links
- **About** — Education, photo collage, career narrative
- **Experience** — Six skill cards (Soft Skills, IT Support, Frontend, Backend, Tools, AI/LLM)
- **Projects** — Ask Rodney AI Chatbot, GitHub repo
- **Ask Rodney** — Conversational AI + Job Match
- **Contact** — Email, LinkedIn

---

## 2. Technology Stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 10.0 |
| Backend | ASP.NET Core (Razor Pages + Web API) |
| Frontend | HTML5, CSS3, JavaScript, Bootstrap |
| AI | OpenAI Chat Completions (gpt-4o-mini) |
| Analytics | Google Analytics 4 |
| Hosting | Azure Web App |
| CI/CD | GitHub Actions |

---

## 3. Architecture Overview

```
User Browser → ASP.NET Core → ChatController / JobMatchService
                                    ↓
                            ResumeContextLoader (Data/ResumeContext.txt)
                                    ↓
                            OpenAI Chat Completions API
```

**Flow:** The chat and job match features load resume context from `Data/ResumeContext.txt`, inject it into the system prompt, and call the OpenAI API. Input validation and content filtering run before any API call.

---

## 4. Project Structure

```
RodneyPortfolio/
├── Controllers/ChatController.cs    # POST /api/chat, /api/chat/job-match
├── Services/                        # OpenAIChatService, JobMatchService, ResumeContextLoader, InputValidator, ContentFilter
├── Models/                         # ChatRequest, ChatResponse, JobMatchRequest, JobMatchResponse
├── Data/ResumeContext.txt          # Resume + about content for AI
├── Pages/Index.cshtml              # Main portfolio page
├── wwwroot/                        # CSS, JS, assets, PWA
├── docs/                           # Technical documentation
└── .github/workflows/              # CI/CD to Azure
```

---

## 5. Features

- **Ask Rodney AI Chatbot** — Answers questions about my resume using OpenAI. Uses static prompt with `ResumeContext.txt`. Falls back to demo responses when the API fails.
- **Job Match** — Paste a job description; get match score, skills aligned, gaps, and talking points.
- **Transparency Panel** — "How this chatbot works" with model, architecture, data, safety, hosting.
- **PWA** — Service worker for offline caching.
- **Input validation** — Max length, prompt injection blocking.
- **Content filtering** — Profanity block list.

---

## 6. API Endpoints

| Endpoint | Method | Request | Response |
|----------|--------|---------|----------|
| `/api/chat` | POST | `{ "message": string }` | `{ "reply": string }` |
| `/api/chat/job-match` | POST | `{ "jobDescription": string }` | `{ matchScore, skillsAligned, gaps, talkingPoints }` |

---

## 7. Configuration

- **Local:** `dotnet user-secrets set "OpenAI:ApiKey" "sk-..."`
- **Production:** GitHub Secret `OPENAI_API_KEY` → Azure App Setting `OpenAI__ApiKey`
- **GA4:** `GoogleAnalytics:MeasurementId` in appsettings or `GA4_MEASUREMENT_ID` GitHub Secret

---

## 8. Documentation

| Document | Description |
|----------|-------------|
| [Complete Technical Documentation](docs/RODNEY_PORTFOLIO_TECHNICAL_DOCUMENTATION.md) | Full A–Z breakdown: HTML, CSS, JS, C#, APIs, architecture |
| [Portfolio AI Skills Implementation](docs/portfolio_ai_skills_implementation_67024d2c.plan.md) | Implementation plan (first-person) |
| [OpenAI API Key Setup](docs/OPENAI_API_KEY_SETUP.md) | Secure API key setup guide |
| [Job Match Feature](docs/JOB_MATCH_FEATURE.md) | Job Match technical docs |
| [Codebase Explained](docs/CODEBASE_EXPLAINED_SIMPLE.md) | Plain-English explanation for recruiters |

---

**Author:** Rodney Chery  
**Repository:** [github.com/ChefRod88/RodneyPortfolio](https://github.com/ChefRod88/RodneyPortfolio)
