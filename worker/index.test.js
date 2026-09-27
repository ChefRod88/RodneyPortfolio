import { describe, it, expect, vi, beforeEach } from "vitest";
import worker from "./index.js";

function makeEnv(overrides = {}) {
  return {
    RESEND_API_KEY: "re_test",
    TURNSTILE_SECRET_KEY: "turnstile_test",
    TO_EMAIL: "rodney@globalrcdev.com",
    FROM_EMAIL: "onboarding@resend.dev",
    QUOTE_RATE_LIMITER: { limit: vi.fn().mockResolvedValue({ success: true }) },
    SUPPORT_RATE_LIMITER: { limit: vi.fn().mockResolvedValue({ success: true }) },
    ASSETS: { fetch: vi.fn().mockResolvedValue(new Response("static asset", { status: 200 })) },
    ...overrides,
  };
}

function formRequest(url, fields) {
  const body = new URLSearchParams(fields);
  return new Request(url, {
    method: "POST",
    headers: { "content-type": "application/x-www-form-urlencoded" },
    body: body.toString(),
  });
}

const validQuoteFields = {
  Name: "Jane Smith",
  Email: "jane@example.com",
  ServiceNeeded: "Business Website",
  EstimatedBudget: "$1,000 – $2,500",
  ProjectDescription: "Need a new site",
  "cf-turnstile-response": "test-token",
};

const validSupportFields = {
  Name: "Jane Smith",
  Email: "jane@example.com",
  Subject: "Contact form broken",
  Message: "It does not submit",
  "cf-turnstile-response": "test-token",
};

function mockFetchSequence({ turnstileSuccess = true, resendOk = true } = {}) {
  global.fetch = vi.fn(async (url) => {
    if (String(url).includes("challenges.cloudflare.com")) {
      return new Response(JSON.stringify({ success: turnstileSuccess, "error-codes": [] }), { status: 200 });
    }
    if (String(url).includes("api.resend.com")) {
      return new Response(JSON.stringify({ id: "email_123" }), { status: resendOk ? 200 : 500 });
    }
    throw new Error(`Unexpected fetch to ${url}`);
  });
}

beforeEach(() => {
  vi.restoreAllMocks();
});

describe("routing", () => {
  it("falls through to ASSETS for unrelated GET requests", async () => {
    const env = makeEnv();
    const res = await worker.fetch(new Request("https://example.com/about"), env);
    expect(env.ASSETS.fetch).toHaveBeenCalledOnce();
    expect(await res.text()).toBe("static asset");
  });
});

describe("Quote handler", () => {
  it("rejects missing required fields before checking rate limit or Turnstile", async () => {
    const env = makeEnv();
    const req = formRequest("https://example.com/?handler=Quote", { Name: "Jane" });
    const res = await worker.fetch(req, env);
    expect(res.status).toBe(400);
    expect(env.QUOTE_RATE_LIMITER.limit).not.toHaveBeenCalled();
  });

  it("rejects an invalid email format", async () => {
    const env = makeEnv();
    const req = formRequest("https://example.com/?handler=Quote", {
      ...validQuoteFields,
      Email: "not-an-email",
    });
    const res = await worker.fetch(req, env);
    expect(res.status).toBe(400);
  });

  it("silently rejects when the honeypot field is filled", async () => {
    const env = makeEnv();
    const req = formRequest("https://example.com/?handler=Quote", {
      ...validQuoteFields,
      Website: "http://spam.example",
    });
    const res = await worker.fetch(req, env);
    expect(res.status).toBe(400);
    const body = await res.json();
    expect(body.ok).toBe(false);
  });

  it("returns 429 when the rate limiter rejects the request", async () => {
    const env = makeEnv({
      QUOTE_RATE_LIMITER: { limit: vi.fn().mockResolvedValue({ success: false }) },
    });
    const req = formRequest("https://example.com/?handler=Quote", validQuoteFields);
    const res = await worker.fetch(req, env);
    expect(res.status).toBe(429);
  });

  it("returns 400 when Turnstile verification fails", async () => {
    mockFetchSequence({ turnstileSuccess: false });
    const env = makeEnv();
    const req = formRequest("https://example.com/?handler=Quote", validQuoteFields);
    const res = await worker.fetch(req, env);
    expect(res.status).toBe(400);
  });

  it("sends the email and returns 200 on a valid submission", async () => {
    mockFetchSequence();
    const env = makeEnv();
    const req = formRequest("https://example.com/?handler=Quote", validQuoteFields);
    const res = await worker.fetch(req, env);
    expect(res.status).toBe(200);
    const body = await res.json();
    expect(body.ok).toBe(true);

    const resendCall = global.fetch.mock.calls.find(([url]) => String(url).includes("api.resend.com"));
    expect(resendCall).toBeTruthy();
    const sentPayload = JSON.parse(resendCall[1].body);
    expect(sentPayload.to).toEqual(["rodney@globalrcdev.com"]);
    expect(sentPayload.reply_to).toBe("jane@example.com");
  });

  it("strips control characters from free-text fields before sending", async () => {
    mockFetchSequence();
    const env = makeEnv();
    const req = formRequest("https://example.com/?handler=Quote", {
      ...validQuoteFields,
      ProjectDescription: "Line one\r\nInjected-Header: evil",
    });
    await worker.fetch(req, env);
    const resendCall = global.fetch.mock.calls.find(([url]) => String(url).includes("api.resend.com"));
    const sentPayload = JSON.parse(resendCall[1].body);
    expect(sentPayload.text).not.toMatch(/[\r\n]Injected-Header/);
  });

  it("returns 500 without sending when RESEND_API_KEY is missing", async () => {
    mockFetchSequence();
    const env = makeEnv({ RESEND_API_KEY: undefined });
    const req = formRequest("https://example.com/?handler=Quote", validQuoteFields);
    const res = await worker.fetch(req, env);
    expect(res.status).toBe(500);
    expect(global.fetch).not.toHaveBeenCalledWith(expect.stringContaining("api.resend.com"), expect.anything());
  });
});

describe("Support handler", () => {
  it("rejects missing required fields", async () => {
    const env = makeEnv();
    const req = formRequest("https://example.com/Support", { Name: "Jane" });
    const res = await worker.fetch(req, env);
    expect(res.status).toBe(400);
  });

  it("sends the email and returns 200 on a valid submission", async () => {
    mockFetchSequence();
    const env = makeEnv();
    const req = formRequest("https://example.com/Support", validSupportFields);
    const res = await worker.fetch(req, env);
    expect(res.status).toBe(200);
    const body = await res.json();
    expect(body.ok).toBe(true);
  });

  it("returns 429 when the support rate limiter rejects the request", async () => {
    const env = makeEnv({
      SUPPORT_RATE_LIMITER: { limit: vi.fn().mockResolvedValue({ success: false }) },
    });
    const req = formRequest("https://example.com/Support", validSupportFields);
    const res = await worker.fetch(req, env);
    expect(res.status).toBe(429);
  });
});

describe("Chat handler", () => {
  it("rejects empty message", async () => {
    const env = makeEnv({ OPENAI_API_KEY: "sk-test" });
    const req = new Request("https://example.com/api/chat", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ message: "   " }),
    });
    const res = await worker.fetch(req, env);
    expect(res.status).toBe(400);
  });

  it("returns 503 when OPENAI_API_KEY is missing", async () => {
    const env = makeEnv({ OPENAI_API_KEY: undefined });
    const req = new Request("https://example.com/api/chat", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ message: "What are your skills?" }),
    });
    const res = await worker.fetch(req, env);
    expect(res.status).toBe(503);
    const body = await res.json();
    expect(body.error).toBe("KEY_NOT_CONFIGURED");
  });

  it("performs vector RAG retrieval and returns 200 with reply and sources", async () => {
    const env = makeEnv({ OPENAI_API_KEY: "sk-test" });
    let capturedPrompt = "";

    global.fetch = vi.fn(async (url, opts) => {
      const urlStr = String(url);
      if (urlStr.includes("api.openai.com/v1/embeddings")) {
        // Return a mock 512-dimension unit vector
        const mockVec = new Array(512).fill(0.04419);
        return new Response(
          JSON.stringify({
            data: [{ embedding: mockVec }],
          }),
          { status: 200, headers: { "content-type": "application/json" } }
        );
      }
      if (urlStr.includes("api.openai.com/v1/chat/completions")) {
        const body = JSON.parse(opts.body);
        capturedPrompt = body.messages[0].content;
        return new Response(
          JSON.stringify({
            choices: [
              {
                message: {
                  content:
                    "Rodney is a Technical Support Specialist III at LeadVenture. He also authored [AI-Assisted Support Engineering Workflow](/Articles/ai-assisted-support-engineering-workflow).",
                },
              },
            ],
          }),
          { status: 200, headers: { "content-type": "application/json" } }
        );
      }
      return new Response("not found", { status: 404 });
    });

    const req = new Request("https://example.com/api/chat", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ message: "Tell me about Rodney's articles and experience" }),
    });
    const res = await worker.fetch(req, env);
    expect(res.status).toBe(200);
    const body = await res.json();
    expect(body.ok).toBe(true);
    expect(body.reply).toContain("Rodney");
    expect(body.sources).toBeDefined();
    expect(Array.isArray(body.sources)).toBe(true);
    expect(body.sources.length).toBeGreaterThan(0);
    expect(capturedPrompt).toContain("RETRIEVED MULTI-DOCUMENT KNOWLEDGE (VECTOR MATCH)");
  });

  it("gracefully falls back to baseline resume when embedding API fails", async () => {
    const env = makeEnv({ OPENAI_API_KEY: "sk-test" });

    global.fetch = vi.fn(async (url) => {
      const urlStr = String(url);
      if (urlStr.includes("api.openai.com/v1/embeddings")) {
        return new Response("OpenAI embeddings quota exceeded", { status: 429 });
      }
      if (urlStr.includes("api.openai.com/v1/chat/completions")) {
        return new Response(
          JSON.stringify({
            choices: [
              {
                message: {
                  content: "Rodney is an enterprise technical support specialist and full-stack developer.",
                },
              },
            ],
          }),
          { status: 200, headers: { "content-type": "application/json" } }
        );
      }
      return new Response("not found", { status: 404 });
    });

    const req = new Request("https://example.com/api/chat", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ message: "What certifications does Rodney hold?" }),
    });
    const res = await worker.fetch(req, env);
    expect(res.status).toBe(200);
    const body = await res.json();
    expect(body.ok).toBe(true);
    expect(body.reply).toContain("Rodney");
  });
});

