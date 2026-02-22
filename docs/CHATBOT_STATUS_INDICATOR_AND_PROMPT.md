# Chatbot Status Indicator and Flexible Prompt

This document describes the API connection status indicator and the flexible system prompt added to the Ask Rodney chatbot.

---

## 1. API Connection Status Indicator

### Purpose

Visitors can see whether the chatbot is using the real OpenAI API or demo mode. A colored dot and label appear above the chat messages.

### Visual States

| Dot Color | Label | Meaning |
|-----------|-------|---------|
| Gray | "Send a message to check connection" | Initial state; no message sent yet |
| Green | "Connected to OpenAI API" | Response came from the real OpenAI API |
| Red | "Demo mode or unable to connect to API" | Response came from demo mode, or there was an error |

### How It Works

1. Each chat response from the API includes a `source` field: `"api"` or `"demo"`.
2. The frontend reads `source` and updates the status indicator.
3. On error (network failure, validation error, etc.), the indicator shows red.

### Backend Implementation

- **ChatResponse model** – Added `Source` property (default: `"demo"`).
- **IAIChatService** – `GetReplyAsync` now returns `(string Reply, string Source)`.
- **OpenAIChatService** – Returns `"api"` when the OpenAI API is called successfully; returns `"demo"` for demo responses and error fallbacks.
- **ChatController** – Sets `Source` on each `ChatResponse`.

### Frontend Implementation

- **Index.cshtml** – Status div with dot and label above chat messages.
- **site.js** – `updateStatus(source, isError)` updates the dot class and label text.
- **site.css** – `.chat-status-api` (green), `.chat-status-demo` (red), `.chat-status-pending` (gray).

### API Response Shape (Updated)

```json
{
  "reply": "Rodney is a Technical Support Inkjet Tier 1 at Canon...",
  "source": "api"
}
```

| Field  | Type   | Description |
|--------|--------|-------------|
| reply  | string | The AI-generated or demo response |
| source | string | `"api"` = OpenAI API; `"demo"` = demo mode or error fallback |

---

## 2. Flexible System Prompt

### Purpose

The chatbot can infer, speculate, and expand on the resume context instead of strictly sticking to the exact text. It stays grounded in the provided information but offers more natural, conversational answers.

### Previous Behavior

- Answer based **only** on the provided information.
- Do not make up facts.
- If asked something not covered, say "I don't have that information."

### Current Behavior

- Use the resume content as the primary source.
- **Infer, speculate, and expand** on it naturally—connect dots, draw reasonable conclusions, offer thoughtful insights.
- **Stay grounded:** Don't invent specific facts (employers, dates, credentials) that contradict or go beyond the context.
- For questions with no direct answer, **offer reasonable inference** or relate to what is known, rather than always saying "I don't have that information."
- Be helpful and conversational.

### Example

**Question:** "How many years has Rodney been at Canon?"

- **Before:** "I don't have that information. Please check his resume or contact him."
- **After:** The model may infer from his career path (kitchens → tech → Canon) and give a more natural answer, or clarify that specific tenure isn't in the context while relating what is known.

### Implementation

- **File:** `Services/OpenAIChatService.cs`
- **Method:** `BuildSystemPrompt(string resumeContext)`
- The prompt text was updated to include the new guidelines.

---

## 3. Files Modified

| File | Change |
|------|--------|
| `Models/ChatResponse.cs` | Added `Source` property |
| `Services/IAIChatService.cs` | Return type: `Task<(string Reply, string Source)>` |
| `Services/OpenAIChatService.cs` | Return source; updated system prompt |
| `Controllers/ChatController.cs` | Set `Source` on response |
| `Pages/Index.cshtml` | Status indicator HTML |
| `wwwroot/js/site.js` | `updateStatus()` and source handling |
| `wwwroot/css/site.css` | Status dot styles |
| `tests/OpenAIChatServiceTests.cs` | Updated for tuple return type |

---

## 4. Related Documentation

- [CHATBOT_API.md](CHATBOT_API.md) – API specification (request/response schema)
- [OPENAI_API_KEY_SETUP.md](OPENAI_API_KEY_SETUP.md) – How to configure the OpenAI API key for local and production
