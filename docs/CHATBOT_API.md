# Ask Rodney Chatbot API Specification

## Overview

The Ask Rodney chatbot allows visitors to ask questions about Rodney Chery's resume, experience, and background. The API is a REST endpoint that accepts a message and returns an AI-generated reply.

## Endpoint

```
POST /api/chat
```

## Request

### Headers

| Header          | Value             |
|-----------------|-------------------|
| Content-Type    | application/json  |

### Body

```json
{
  "message": "What's Rodney's experience?"
}
```

| Field    | Type   | Required | Description                    |
|----------|--------|----------|--------------------------------|
| message  | string | Yes      | The visitor's question. Max 500 characters. |

## Response

### Success (200 OK)

```json
{
  "reply": "Rodney is a Technical Support Inkjet Tier 1 at Canon Information Technology Services. He troubleshoots proprietary software, hardware environments, and enterprise applications by systematically isolating variables and guiding issues through structured resolution."
}
```

| Field  | Type   | Description                    |
|--------|--------|--------------------------------|
| reply  | string | The AI-generated response      |

### Error Responses

| Status | Description |
|--------|-------------|
| 400 Bad Request | Invalid input: empty message, exceeds max length (500 chars), contains blocked patterns, or fails content filter |
| 500 Internal Server Error | Server or AI service error |

#### 400 Error Body

```json
{
  "error": "Message must be 500 characters or less."
}
```

#### 500 Error Body

```json
{
  "error": "Something went wrong. Please try again."
}
```

## Input Validation

- **Max length:** 500 characters
- **Blocked patterns:** Prompt injection attempts (e.g., "ignore previous", "system:", "disregard") are rejected
- **Content filter:** Inappropriate content is blocked

## Demo Mode

When no OpenAI API key is configured (or `UseDemoMode` is true), the service returns predefined responses based on keyword matching. This allows the chatbot UI to work for visitors without incurring API costs.

## Configuration

Configure in `appsettings.json` or User Secrets:

```json
{
  "OpenAI": {
    "ApiKey": "sk-...",
    "Model": "gpt-4o-mini",
    "UseDemoMode": false
  }
}
```

- **ApiKey:** Your OpenAI API key. Leave empty for demo mode.
- **Model:** OpenAI model (default: gpt-4o-mini)
- **UseDemoMode:** When true, uses canned responses even if ApiKey is set

## Resume Context

The AI uses content from `Data/ResumeContext.txt` as its knowledge base. Update this file when you update your resume.
