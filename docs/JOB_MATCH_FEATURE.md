# Job Match Feature — Technical Documentation

## Overview

I built the **Job Match** feature so recruiters and hiring managers can paste a job description into my portfolio and get an AI-powered analysis of how my resume aligns with the role. It returns a match score, skills alignment, gaps, and suggested talking points for interviews. Let me walk you through how it works.

---

## Architecture

Here's the flow from the UI to the API:

```
┌─────────────────┐     POST /api/chat/job-match      ┌──────────────────┐
│  Job Match UI    │ ──────────────────────────────► │ ChatController   │
│  (Index.cshtml)  │     { jobDescription: string }    │                  │
└─────────────────┘                                   └────────┬─────────┘
                                                               │
                                                               ▼
┌─────────────────┐     JobMatchResponse              ┌──────────────────┐
│  Result Display │ ◄──────────────────────────────── │ JobMatchService  │
│  (site.js)      │     matchScore, skillsAligned,   │                  │
└─────────────────┘     gaps, talkingPoints           └────────┬─────────┘
                                                               │
                                                               ▼
                                                      ┌──────────────────┐
                                                      │ OpenAI API        │
                                                      │ Chat Completions  │
                                                      │ (gpt-4o-mini)     │
                                                      └──────────────────┘
```

---

## API Specification

### Endpoint

| Method | Path | Description |
|--------|------|--------------|
| POST | `/api/chat/job-match` | Analyzes job description against my resume |

### Request

**Content-Type:** `application/json`

```json
{
  "jobDescription": "string (required, max 4000 characters)"
}
```

**Validation:**
- `jobDescription` is required
- Cannot be empty (after trim)
- Max length: 4,000 characters

**Error responses:**
- `400` — Missing, empty, or too long job description
- `500` — Server error during analysis

### Response (Success)

**Status:** `200 OK`

```json
{
  "matchScore": 75,
  "skillsAligned": ["C#", "ASP.NET Core", "SQL Server", "Git"],
  "gaps": ["Kubernetes", "5+ years experience"],
  "talkingPoints": ["Emphasize Canon enterprise support experience", "Highlight OpenAI integration project"]
}
```

| Field | Type | Description |
|-------|------|-------------|
| `matchScore` | `int` | 0–100 compatibility score |
| `skillsAligned` | `string[]` | Job requirements I have |
| `gaps` | `string[]` | Job requirements I lack or am weak in |
| `talkingPoints` | `string[]` | Suggested interview talking points |

---

## Backend Components

### 1. ChatController (`Controllers/ChatController.cs`)

- **Action:** `JobMatch([FromBody] JobMatchRequest request)`
- **What it does:** Validates the request (required, non-empty, max 4000 chars), calls `IJobMatchService.AnalyzeAsync`, and returns the `JobMatchResponse` or an error.

### 2. JobMatchService (`Services/JobMatchService.cs`)

- **Dependencies:** `IResumeContextLoader`, `IConfiguration`, `IHttpClientFactory`, `ILogger`
- **Flow:**
  1. I load my resume context from `Data/ResumeContext.txt`
  2. I build a structured prompt with the job description and my resume
  3. I call the OpenAI Chat Completions API with `response_format: { type: "json_object" }`
  4. I parse the JSON response into `JobMatchResponse`
  5. I handle errors (missing API key, API failure, parse failure) with fallback responses

### 3. Models

**JobMatchRequest** (`Models/JobMatchRequest.cs`):
```csharp
public class JobMatchRequest
{
    [Required]
    public string JobDescription { get; set; } = string.Empty;
}
```

**JobMatchResponse** (`Models/JobMatchResponse.cs`):
```csharp
public class JobMatchResponse
{
    public int MatchScore { get; set; }
    public List<string> SkillsAligned { get; set; } = new();
    public List<string> Gaps { get; set; } = new();
    public List<string> TalkingPoints { get; set; } = new();
}
```

---

## OpenAI Integration

- **Model:** `gpt-4o-mini` (from config `OpenAI:Model`)
- **API Key:** Same as the chatbot — `OpenAI:ApiKey` (User Secrets locally, GitHub Secrets in production)
- **JSON Mode:** `response_format: { type: "json_object" }` ensures I get structured output
- **Max Tokens:** 1024

**Prompt structure:**
1. I instruct the model to compare the job description vs. my resume
2. I require JSON with keys: `matchScore`, `skillsAligned`, `gaps`, `talkingPoints`
3. I provide the job description and resume context in delimited blocks

---

## Frontend Components

### UI (Index.cshtml)

- Textarea for job description (maxlength 4000)
- Character counter
- "Analyze" button
- Result section (hidden until response):
  - Match score (0–100%)
  - Skills Aligned list
  - Gaps list
  - Talking Points list

### JavaScript (site.js — `initJobMatch()`)

- `fetch` to `POST /api/chat/job-match` with `{ jobDescription }`
- Renders `matchScore`, `skillsAligned`, `gaps`, `talkingPoints` in the DOM
- Handles errors via `alert`
- Character count updates on input

### CSS (site.css)

- `.chat-job-match` — Container styling
- `.chat-job-match-input` — Textarea
- `.chat-job-match-btn` — Analyze button
- `.chat-job-match-result` — Result section
- `.chat-job-match-score` — Score display
- `.chat-job-match-lists` — Grid for skills/gaps/talking points

---

## Error Handling

| Scenario | Behavior |
|----------|----------|
| No API key configured | Returns `MatchScore: 0`, `Gaps: ["Chatbot is not configured."]` |
| OpenAI API failure (401, 403, 429, 500) | Returns `MatchScore: 0`, `Gaps: ["Unable to analyze. Please try again later."]` |
| Parse failure | Returns `MatchScore: 0`, `Gaps: ["Could not parse analysis. Please try again."]` |
| Network exception | Same as API failure; logged and fallback returned |

---

## Dependency Injection

I register the service in `Program.cs`:

```csharp
builder.Services.AddScoped<IJobMatchService, JobMatchService>();
```

---

## File Summary

| File | Purpose |
|------|---------|
| `Controllers/ChatController.cs` | Job Match API action |
| `Services/IJobMatchService.cs` | Service interface |
| `Services/JobMatchService.cs` | OpenAI integration, prompt, parsing |
| `Models/JobMatchRequest.cs` | Request DTO |
| `Models/JobMatchResponse.cs` | Response DTO |
| `Pages/Index.cshtml` | Job Match UI markup |
| `wwwroot/js/site.js` | `initJobMatch()` — fetch and render |
| `wwwroot/css/site.css` | Job Match styles |

---

## Future Enhancements

- **Embeddings:** Use semantic similarity (e.g., Azure AI Search) for richer matching
- **Caching:** Cache results for identical job descriptions
- **Rate limiting:** Limit requests per IP to prevent abuse
