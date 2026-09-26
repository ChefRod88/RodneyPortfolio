// Handles the Quote and Support form POSTs that the frozen static site can no
// longer reach now that the ASP.NET backend only runs at build time (see
// docs/migration/azure-to-cloudflare.md). Everything else falls through to
// the static assets, unchanged.

const JSON_HEADERS = { "content-type": "application/json" };

function jsonResponse(status, body) {
  return new Response(JSON.stringify(body), { status, headers: JSON_HEADERS });
}

// Port of Services/QuoteSanitizer.cs — strips control chars used for header/log
// injection and collapses whitespace. Keep in sync if that file changes.
function sanitize(value) {
  if (!value) return "";
  const cleaned = String(value)
    .replace(/[\r\n\0\x01-\x08\x0b\x0c\x0e-\x1f\x7f]/g, " ")
    .replace(/[ \t]{2,}/g, " ");
  return cleaned.trim();
}

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

function missingRequired(fields) {
  for (const [name, value] of Object.entries(fields)) {
    if (!value || !String(value).trim()) return name;
  }
  return null;
}

async function checkRateLimit(env, bindingName, ip) {
  const limiter = env[bindingName];
  if (!limiter) return true; // binding not configured (e.g. local dev without it) — fail open
  const { success } = await limiter.limit({ key: ip });
  return success;
}

async function verifyTurnstile(env, token, ip) {
  if (!env.TURNSTILE_SECRET_KEY) {
    return { success: false, errorCodes: ["missing-config-secret"] };
  }
  if (!token) {
    return { success: false, errorCodes: ["missing-token"] };
  }

  const body = new URLSearchParams({
    secret: env.TURNSTILE_SECRET_KEY,
    response: token,
  });
  if (ip) body.set("remoteip", ip);

  const res = await fetch("https://challenges.cloudflare.com/turnstile/v0/siteverify", {
    method: "POST",
    headers: { "content-type": "application/x-www-form-urlencoded" },
    body,
  });
  if (!res.ok) {
    return { success: false, errorCodes: ["verify-http-failed"] };
  }
  const payload = await res.json();
  return { success: !!payload.success, errorCodes: payload["error-codes"] ?? [] };
}

async function sendEmail(env, { subject, text, replyTo }) {
  if (!env.RESEND_API_KEY || !env.TO_EMAIL) {
    throw new Error("Email is not configured. Set RESEND_API_KEY and TO_EMAIL.");
  }

  const res = await fetch("https://api.resend.com/emails", {
    method: "POST",
    headers: {
      authorization: `Bearer ${env.RESEND_API_KEY}`,
      "content-type": "application/json",
    },
    body: JSON.stringify({
      from: env.FROM_EMAIL || "onboarding@resend.dev",
      to: [env.TO_EMAIL],
      reply_to: replyTo,
      subject,
      text,
    }),
  });

  if (!res.ok) {
    const detail = await res.text().catch(() => "");
    throw new Error(`Resend request failed (${res.status}): ${detail}`);
  }
}

async function handleQuote(request, env, ip) {
  const form = await request.formData();

  const website = form.get("Website");
  if (website && String(website).trim()) {
    return jsonResponse(400, { ok: false, message: "Unable to submit request." });
  }

  const fields = {
    Name: form.get("Name"),
    Email: form.get("Email"),
    ServiceNeeded: form.get("ServiceNeeded"),
    EstimatedBudget: form.get("EstimatedBudget"),
    ProjectDescription: form.get("ProjectDescription"),
  };
  const missing = missingRequired(fields);
  const email = String(fields.Email || "");
  if (missing || !EMAIL_RE.test(email)) {
    return jsonResponse(400, { ok: false, message: "Please complete all required fields." });
  }

  const allowed = await checkRateLimit(env, "QUOTE_RATE_LIMITER", ip);
  if (!allowed) {
    return jsonResponse(429, { ok: false, message: "Too many requests. Please try again later." });
  }

  const verify = await verifyTurnstile(env, form.get("cf-turnstile-response"), ip);
  if (!verify.success) {
    return jsonResponse(400, { ok: false, message: "Please complete the CAPTCHA and try again." });
  }

  const safe = {
    name: sanitize(fields.Name),
    email: sanitize(fields.Email),
    company: sanitize(form.get("Company")),
    serviceNeeded: sanitize(fields.ServiceNeeded),
    estimatedBudget: sanitize(fields.EstimatedBudget),
    projectDescription: sanitize(fields.ProjectDescription),
    timeline: sanitize(form.get("Timeline")),
  };

  const text = [
    "New quote request submitted.",
    "",
    `Name: ${safe.name}`,
    `Email: ${safe.email}`,
    `Company: ${safe.company}`,
    `Service Needed: ${safe.serviceNeeded}`,
    `Estimated Budget: ${safe.estimatedBudget}`,
    `Project Description: ${safe.projectDescription}`,
    `Timeline: ${safe.timeline}`,
  ].join("\n");

  try {
    await sendEmail(env, {
      subject: `New Quote Request - ${safe.name}`,
      text,
      replyTo: safe.email,
    });
    return jsonResponse(200, { ok: true });
  } catch (err) {
    console.error("Failed to send quote request email", err);
    return jsonResponse(500, { ok: false, message: "Unable to submit right now. Please email directly." });
  }
}

async function handleSupport(request, env, ip) {
  const form = await request.formData();

  const website = form.get("Website");
  if (website && String(website).trim()) {
    return jsonResponse(400, { ok: false, message: "Unable to submit request." });
  }

  const fields = {
    Name: form.get("Name"),
    Email: form.get("Email"),
    Subject: form.get("Subject"),
    Message: form.get("Message"),
  };
  const missing = missingRequired(fields);
  const email = String(fields.Email || "");
  if (missing || !EMAIL_RE.test(email)) {
    return jsonResponse(400, { ok: false, message: "Please complete all required fields." });
  }

  const allowed = await checkRateLimit(env, "SUPPORT_RATE_LIMITER", ip);
  if (!allowed) {
    return jsonResponse(429, { ok: false, message: "Too many requests. Please try again later." });
  }

  const verify = await verifyTurnstile(env, form.get("cf-turnstile-response"), ip);
  if (!verify.success) {
    return jsonResponse(400, { ok: false, message: "Please complete the CAPTCHA and try again." });
  }

  const safe = {
    name: sanitize(fields.Name),
    email: sanitize(fields.Email),
    siteOrProject: sanitize(form.get("SiteOrProject")),
    subject: sanitize(fields.Subject),
    message: sanitize(fields.Message),
  };

  const text = [
    "New public support request submitted.",
    "",
    `Name: ${safe.name}`,
    `Email: ${safe.email}`,
    `Site/Project: ${safe.siteOrProject}`,
    `Subject: ${safe.subject}`,
    "",
    "Message:",
    safe.message,
  ].join("\n");

  try {
    await sendEmail(env, {
      subject: `[RC Dev Support] ${safe.subject} — ${safe.name}`,
      text,
      replyTo: safe.email,
    });
    return jsonResponse(200, { ok: true });
  } catch (err) {
    console.error("Failed to send support request email", err);
    return jsonResponse(500, {
      ok: false,
      message: "Unable to submit right now. Please email rodney@globalrcdev.com directly.",
    });
  }
}

export default {
  async fetch(request, env) {
    const url = new URL(request.url);
    const ip = request.headers.get("CF-Connecting-IP");

    if (request.method === "POST" && url.pathname === "/" && url.searchParams.get("handler") === "Quote") {
      return handleQuote(request, env, ip);
    }

    if (request.method === "POST" && (url.pathname === "/Support" || url.pathname === "/Support/")) {
      return handleSupport(request, env, ip);
    }

    return env.ASSETS.fetch(request);
  },
};
