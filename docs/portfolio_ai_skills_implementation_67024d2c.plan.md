---
name: Portfolio AI Skills Implementation
overview: Add an AI-powered 'Ask Rodney' chatbot to your portfolio that answers questions about you and your resume. Visitors can ask anything about your background, experience, skills, and approach—and get natural-language responses powered by an LLM.
todos: []
isProject: false
---

# Portfolio AI Skills Implementation Plan

## What I Built: "Ask Rodney"

I built this chatbot so visitors to my portfolio can ask questions about me directly. Instead of scrolling through my resume, they can type things like "What's Rodney's background?" or "Tell me about his experience at Canon" and get a natural answer. The AI uses my resume content and About Me section as its knowledge base, so it answers accurately and in my voice.

---

## How It Works (Overview)

Here's the flow: a visitor types a question in the chat interface. That sends a request to my backend API. The backend validates the input, filters out anything inappropriate, then calls the OpenAI API with a system prompt that includes my resume. The AI returns a response, and we display it in the chat. Simple, but it demonstrates several skills I care about: LLM integration, REST APIs, prompt engineering, and AI safety.

```
Visitor types question → Chat UI → POST /api/chat → Validate → Filter → AI Service → OpenAI API → Response back to UI
```

---

## Step-by-Step: What I Did

### Step 1: Configuration

First, I set up the configuration. I needed a place to store the OpenAI API key (securely) and the model name. I added an `OpenAI` section to `appsettings.json` with `ApiKey`, `Model` (I use `gpt-4o-mini` for cost), and `UseDemoMode`. When `UseDemoMode` is true and there's no API key, the chatbot returns predefined responses instead of calling the real API. That way visitors can use it even without me paying for API calls—great for demo.

### Step 2: The AI Chat Service

Next, I created the core service that talks to the AI. I defined an interface `IAIChatService` and implemented `OpenAIChatService`. The service loads my resume content from a text file (`Data/ResumeContext.txt`), builds a system prompt that tells the AI to answer only from that content, and sends it to the OpenAI API. The prompt is the key part—that's prompt engineering. I instruct the AI to speak in third person about me, keep responses concise, and never make up facts.

### Step 2b: Resume Context

I created `Data/ResumeContext.txt` with all the key info from my resume: name, title, education, experience, skills, and a summary of my about section. I update this file whenever I update my resume. That way the AI always has accurate, up-to-date information to work with.

### Step 3: Input Validation and Content Filtering

Before any message reaches the AI, I validate and filter it. I built `InputValidator` to enforce a 500-character limit and block common prompt injection patterns (like "ignore previous" or "system:"). I also built `ContentFilter` to block inappropriate content. This is important for AI safety—especially when you're building something that could be used in a public or government context.

### Step 4: The Chat API

I created a REST API controller at `POST /api/chat`. The frontend sends a JSON body like `{ "message": "What's Rodney's background?" }`. The controller validates the input, runs it through the content filter, calls the AI service, and returns `{ "reply": "..." }`. This is the standard RESTful pattern: JSON in, JSON out.

### Step 5: The Chat UI

I added a new section to my portfolio page called "Ask Rodney" or "Chat with Rodney." It has a scrollable message area, suggested questions (like "What's his experience?" or "What skills does he have?"), a text input, and a send button. The JavaScript uses `fetch` to call the API, displays the user's message, shows a loading state, then renders the AI's reply. It's a simple, clean conversational interface.

### Step 6: Wiring Everything Up

I registered the services in `Program.cs`—the AI service, the resume context loader, and the HTTP client. I also added `MapControllers()` so the API routes work. That's dependency injection and routing in ASP.NET Core.

### Step 7: Skills Section

I added an "AI / LLM Development" skills card to my Experience section. It lists: AI/LLM API Integration, Conversational Interfaces, Prompt Engineering, RESTful APIs & JSON, and Input Validation & Filtering. It aligns my portfolio with the language I see in AI application development job postings.

### Step 8: Project Card

I added a project card for the chatbot in my Projects section. It links to the chatbot demo (anchor `#ask-rodney`) and has a short description of what it does—AI-powered, answers questions about my resume, built with C#, prompt engineering, and input validation.

### Step 9: Documentation

I wrote inline comments in the code and created `docs/CHATBOT_API.md` with the API spec. I also added a brief "How to use" note in the chatbot UI. The job I'm targeting requires technical documentation, so I made sure to cover that.

### Step 10: Unit Tests (Required)

I created an xUnit test project and wrote tests for the input validator, content filter, and AI service (demo mode). The job requires unit and integration tests for AI features, so I treated this as a must, not optional.

---

## Demo Mode and API Keys

**Demo mode:** When there's no API key (or `UseDemoMode` is true), the service returns predefined responses based on keyword matching. If someone asks about "experience", they get a summary. If they ask about "education", they get a summary. The UI works for everyone, even without a key.

**Production:** When you add an API key (via User Secrets or Azure Key Vault), set `UseDemoMode` to false and the chatbot will call the real OpenAI API for natural, varied responses.

---

## Implementation Order

If you're building this yourself, here's the order I'd recommend:

1. Config + ResumeContext + AI service (with demo mode)
2. Validation + Filter + API controller
3. Chat UI
4. DI and routing in Program.cs
5. Skills section + Project card
6. Documentation
7. Unit tests

---

## Files I Created or Modified

| Action | Path |
| ------ | ---- |
| Modify | `RodneyPortfolio.csproj`, `Program.cs`, `appsettings.json` |
| Create | `Services/IAIChatService.cs`, `Services/OpenAIChatService.cs`, `Data/ResumeContext.txt` |
| Create | `Services/InputValidator.cs`, `Services/ContentFilter.cs` |
| Create | `Controllers/ChatController.cs`, `Models/ChatRequest.cs`, `Models/ChatResponse.cs` |
| Modify | `Pages/Index.cshtml`, `wwwroot/css/site.css`, `wwwroot/js/site.js` |
| Create | `docs/CHATBOT_API.md` |
| Create | `Tests/` project and test classes |
