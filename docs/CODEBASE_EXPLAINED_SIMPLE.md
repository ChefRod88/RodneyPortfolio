# My Codebase Explained (Simple Version)

A guide to understand your portfolio chatbot so you can explain it to recruiters in plain English.

---

## The Big Picture: What Does This Thing Do?

**In one sentence:** When someone visits your website and types a question about you in the chat box, your app either asks OpenAI to answer it (using your resume) or gives a pre-written answer if the API isn't connected.

**Like a 10-year-old:** Imagine you have a robot friend who knows everything about you because you gave it a cheat sheet (your resume). When someone asks the robot "What does Rodney do?", the robot reads the cheat sheet and answers. Sometimes the robot is "smart" (calls OpenAI). Sometimes it's "on autopilot" (uses pre-written answers). A little light tells you which mode it's in.

---

## The Journey of a Message (Step by Step)

### 1. Someone types a question

**Where:** The chat box on your website (built with HTML in `Index.cshtml`).

**What happens:** JavaScript (`site.js`) watches for the Send button or Enter key. When the user sends a message, it gets sent to your server.

**Simple analogy:** Like dropping a letter in a mailbox. The letter says "What's Rodney's experience?"

---

### 2. The message arrives at your server

**Where:** `ChatController.cs` – this is the "front desk" of your API.

**What it does:**
1. **Checks the message isn't empty** – "Did you actually write something?"
2. **Checks it's not too long** – Max 500 characters (so nobody sends a novel).
3. **Checks for bad words** – `ContentFilter` blocks profanity.
4. **Checks for "hacker" tricks** – `InputValidator` blocks phrases like "ignore previous instructions" (prompt injection).

**Simple analogy:** Like a bouncer at a club. They make sure you're not carrying anything dangerous before you get in.

**What it does NOT do:** It doesn't answer the question yet. It just decides if the message is safe to pass along.

---

### 3. The message goes to the "brain"

**Where:** `OpenAIChatService.cs` – this is the actual AI logic.

**What it does:**

**Option A – API mode (when you have a key):**
1. Loads your resume from `Data/ResumeContext.txt`.
2. Builds a "system prompt" – instructions that tell the AI: "You're Rodney's assistant. Here's his resume. Answer questions based on this. You can infer and expand, but don't make stuff up."
3. Sends your resume + the user's question to OpenAI's API.
4. Gets back an answer and returns it.

**Option B – Demo mode (no key or key not working):**
1. Looks at the question for keywords: "experience", "skills", "education", "canon", etc.
2. Picks a pre-written answer that matches.
3. Returns that answer. No internet call to OpenAI.

**Simple analogy:** 
- **API mode** = Asking a smart friend who read your resume to answer.
- **Demo mode** = Using flash cards. If the question mentions "experience", you pull out the "experience" card and read it.

---

### 4. The answer goes back to the user

**Where:** `ChatController` sends the reply. JavaScript receives it and shows it in the chat.

**What the user sees:** Their question, then "Thinking...", then the answer. Plus a green or red dot showing whether it came from the real API or demo mode.

---

## What Each File Does (Quick Reference)

| File | Job | Simple analogy |
|------|-----|----------------|
| `Program.cs` | Starts the app, wires up services, handles HTTPS redirects | The manager who opens the store and sets the rules |
| `ChatController.cs` | Receives messages, checks them, sends to AI service, returns answers | The front desk / receptionist |
| `OpenAIChatService.cs` | Calls OpenAI or uses demo answers | The brain that actually answers |
| `ResumeContextLoader.cs` | Reads your resume from a file | The person who fetches the cheat sheet |
| `InputValidator.cs` | Blocks long messages and prompt injection | The bouncer checking for tricks |
| `ContentFilter.cs` | Blocks bad words | The bouncer checking for inappropriate stuff |
| `ChatRequest.cs` | The shape of "what the user sent" | The envelope the letter comes in |
| `ChatResponse.cs` | The shape of "what we send back" (reply + source) | The envelope we send back with the answer |
| `site.js` | Sends messages, shows answers, updates the status dot | The mail carrier + the person who displays the answer |
| `Index.cshtml` | The chat UI (box, buttons, messages area) | The physical chat window on the page |

---

## What This DOES

- Answers questions about you (background, experience, skills, education, approach).
- Uses your resume as the source of truth.
- Works even without an API key (demo mode).
- Blocks inappropriate content and prompt injection.
- Shows a green/red dot so you know if the real API is connected.
- Limits messages to 500 characters.
- Runs on Azure, deploys via GitHub Actions.
- Keeps the API key secret (User Secrets locally, GitHub Secrets in production).

---

## What This DOES NOT Do

- **Does NOT** remember previous messages. Each question is standalone (no conversation history).
- **Does NOT** answer questions about other people or random topics. It's focused on you.
- **Does NOT** store chat history in a database. Messages disappear when you refresh.
- **Does NOT** let users log in or create accounts.
- **Does NOT** send emails or notifications.
- **Does NOT** scrape the web or use external data besides your resume file.
- **Does NOT** support file uploads or images in the chat.

---

## Recruiter Talking Points

**"Walk me through your chatbot."**

> "Visitors can ask questions about me in a chat box. The frontend sends the message to a REST API. The controller validates it—length, content filter, prompt injection checks—then passes it to the AI service. The service either calls OpenAI with my resume as context or uses pre-written demo answers if the API isn't configured. The response includes a source flag so the UI can show a green or red connection indicator."

**"How did you handle security?"**

> "Input validation limits length and blocks prompt injection patterns. A content filter blocks inappropriate language. The API key is never in the codebase—it's in User Secrets locally and GitHub Secrets in production, injected as environment variables at deploy time."

**"Why use an interface for the AI service?"**

> "ChatController depends on IAIChatService, not OpenAIChatService directly. That way I can swap implementations—for example, a different AI provider or a mock for testing—without changing the controller. It's dependency injection and loose coupling."

**"What happens when the API fails?"**

> "We catch the exception, log it, and return a friendly fallback message. The status indicator turns red so the user knows we're in demo mode. We never expose stack traces or internal errors to the user."

**"How does demo mode work?"**

> "It's keyword matching. If the question contains 'experience' or 'canon', we return the experience paragraph. Same for skills, education, background, etc. It's simple but lets the UI work for visitors even without an API key."

---

## One-Liner Summary

**For you:** "It's a chatbot that answers questions about me using my resume. It calls OpenAI when configured, falls back to pre-written answers otherwise, and includes validation, filtering, and a connection status indicator."

**For a 10-year-old:** "It's a robot that knows about me and answers questions. Sometimes it asks a super-smart computer for help. Sometimes it uses flash cards. A light tells you which one."
