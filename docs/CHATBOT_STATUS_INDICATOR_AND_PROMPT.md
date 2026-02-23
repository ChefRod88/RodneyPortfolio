# Chatbot Flexible Prompt

This document describes the flexible system prompt used by the Ask Rodney chatbot. The chatbot calls the OpenAI API exclusively (no demo mode). An API key is required.

---

## Flexible System Prompt

### Purpose

The chatbot can infer, speculate, and expand on the resume context instead of strictly sticking to the exact text. It stays grounded in the provided information but offers more natural, conversational answers.

### Behavior

- Use the resume content as the primary source.
- **Infer, speculate, and expand** on it naturally—connect dots, draw reasonable conclusions, offer thoughtful insights.
- **Stay grounded:** Don't invent specific facts (employers, dates, credentials) that contradict or go beyond the context.
- For questions with no direct answer, **offer reasonable inference** or relate to what is known, rather than always saying "I don't have that information."
- Be helpful and conversational.
- **Answer the SPECIFIC question asked.** Different questions deserve different answers (e.g., "Where does he work?" gets a short answer; "What's his experience?" gets a fuller narrative).

### Implementation

- **File:** `Services/OpenAIChatService.cs`
- **Method:** `BuildSystemPrompt(string resumeContext)`
- Current date is injected for tenure calculations (e.g., "How long has he been at Canon?").

---

## Related Documentation

- [CHATBOT_API.md](CHATBOT_API.md) – API specification (request/response schema)
- [OPENAI_API_KEY_SETUP.md](OPENAI_API_KEY_SETUP.md) – How to configure the OpenAI API key for local and production
