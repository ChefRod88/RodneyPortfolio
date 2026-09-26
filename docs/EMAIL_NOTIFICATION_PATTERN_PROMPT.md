# LLM Prompt: Add Transactional Email (Resend + Cloudflare Worker) to an Existing System

Paste this whole document to an LLM/agent working in the target codebase (e.g.
an EMR system with existing Appointment and Feedback modules). It describes a
production-validated pattern — built and deployed for a portfolio site's
Quote/Support forms — generalized so it can be dropped into any system that
needs to send transactional email without standing up SMTP infrastructure.

---

## STOP — read this before touching an EMR/PHI system

This pattern was originally built for a **non-PHI** contact form. Before
reusing it for appointments, feedback, or anything tied to a real patient
record, resolve these first — they change the design, not just the config:

1. **Does the email body contain PHI?** (patient name + appointment type +
   provider + diagnosis/treatment detail, etc.) If yes:
   - You need a **Business Associate Agreement (BAA)** with Resend (or
     whatever ESP you use) before sending a single message containing PHI.
     Confirm current BAA availability directly with the provider — do not
     assume it's covered by a standard plan.
   - Prefer **PHI-minimal notifications** instead: *"You have a new message
     in your patient portal — appointment update"* with a link to log in,
     rather than embedding the appointment/diagnosis details in the email
     body itself. This sidesteps the BAA requirement entirely for most flows
     and is the safer default.
   - If the target account/project already has a naming convention like
     `*-non-phi-forms` for a Worker, that's a signal a prior decision was
     made to keep PHI out of this exact pipeline — check for that pattern
     before assuming email content should include patient specifics.
2. **Rate limiting and Turnstile still apply**, but do not treat them as a
   substitute for auth. Appointment/feedback endpoints in an EMR should
   already require an authenticated session (patient portal, staff login) —
   this pattern's honeypot + Turnstile is designed for *public, unauthenticated*
   forms like a marketing site's contact form. Don't bolt Turnstile onto an
   authenticated in-app action as if it replaces the auth check.
3. **Audit logging**: EMR systems typically need an audit trail of who
   triggered a notification and when (HIPAA accounting of disclosures, if the
   message contains PHI). This pattern as built has no audit log — add one
   before shipping if this sends anything PHI-adjacent.

If none of the above applies (e.g., this is a purely operational notification
with zero patient-identifying content — "the system had an error", an internal
staff alert, a non-PHI marketing/newsletter opt-in), proceed with the pattern
as-is.

---

## The pattern, in one paragraph

A single edge function (Cloudflare Worker) intercepts specific POST routes,
validates the payload server-side, checks a honeypot field and a per-IP rate
limit, optionally verifies a CAPTCHA (Turnstile), sanitizes free-text fields
against header/log injection, then calls the Resend HTTP API directly with a
plain `fetch()` — no SMTP client, no mail server, no queue infrastructure. It
falls through to normal app routing for everything else. Total dependency
footprint: one API key.

## Why this is worth reusing

- **No SMTP infra to run or secure.** One `fetch()` call, one API key.
- **Runs at the edge**, next to wherever the rest of the app/site is served —
  no separate email microservice to deploy or monitor.
- **Secrets never touch the request path or logs** — stored as platform
  secrets (`wrangler secret put` or the host's equivalent), read only inside
  the handler.
- **Fails loud, not silent** — a misconfigured key throws before attempting
  the send, so it surfaces as a 500 in monitoring rather than a silently
  dropped email.
- **Delivery is observable** — Resend's dashboard shows every send's status
  (`delivered`/`bounced`/`failed`) without needing to build your own logging.

## Reference implementation (generalize, don't copy verbatim)

```js
// Generic building blocks — adapt names/fields per notification type.

function sanitize(value) {
  if (!value) return "";
  return String(value)
    .replace(/[\r\n\0\x01-\x08\x0b\x0c\x0e-\x1f\x7f]/g, " ")
    .replace(/[ \t]{2,}/g, " ")
    .trim();
}

async function checkRateLimit(env, bindingName, key) {
  const limiter = env[bindingName];
  if (!limiter) return true; // fail open only if binding truly isn't configured
  const { success } = await limiter.limit({ key });
  return success;
}

async function sendEmail(env, { to, subject, text, replyTo }) {
  if (!env.RESEND_API_KEY) {
    throw new Error("Email is not configured. Set RESEND_API_KEY.");
  }
  const res = await fetch("https://api.resend.com/emails", {
    method: "POST",
    headers: {
      authorization: `Bearer ${env.RESEND_API_KEY}`,
      "content-type": "application/json",
    },
    body: JSON.stringify({
      from: env.FROM_EMAIL,
      to: [to],
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
```

Each notification type (appointment confirmed, appointment reminder,
appointment canceled, feedback received, feedback response ready) becomes one
small handler that: validates its specific required fields → builds a
`subject`/`text` → calls `sendEmail()`. Keep the validation and rate-limit
scaffolding shared; keep the field lists and copy per-notification-type.

## Integration points to look for in an EMR-style system

When you (the LLM) are handed this prompt inside the target repo, go find:

1. **Appointment module** — where is an appointment created/updated/canceled?
   That's the trigger point for confirmation/reminder/cancellation emails.
   Check whether reminders need to be *scheduled* (not just triggered on
   create) — that likely needs a Cron Trigger or queue, not just a fetch
   handler, since Workers don't have long-running timers.
2. **Feedback module** — where is feedback submitted, and where is a
   response/resolution recorded? Two notification points: "we got your
   feedback" (to the submitter) and "new feedback needs review" (to staff).
3. **Existing notification/messaging code** — search for any existing
   `EmailService`, `NotificationService`, SMTP config, or `IEmailSender`
   interface before adding a parallel system. If one exists, this Worker
   pattern should either replace it cleanly or be scoped to routes the
   existing service doesn't cover — don't run two competing email paths.
4. **Existing auth/session middleware** — confirm which of the trigger
   points are authenticated app actions (most EMR flows) vs. public forms
   (rare in an EMR, but check) so you apply the STOP-section guidance
   correctly per-endpoint.

## Config checklist (mirrors what we set up for the reference deployment)

| Item | Notes |
|---|---|
| `RESEND_API_KEY` | Platform secret, "Sending access" scope only |
| `FROM_EMAIL` | Must be on a **verified domain** — `onboarding@resend.dev`-style shared senders only deliver to the ESP account owner, unusable for real patient-facing mail |
| Sending domain DNS (SPF/DKIM/MX) | Verify in the ESP dashboard, add records at the authoritative DNS provider |
| Rate limiting | Per-IP for public endpoints; per-authenticated-user for in-app actions to prevent notification spam from a compromised/scripted session |
| BAA with ESP | **Required before first PHI-containing send** — see STOP section |
| Audit log of sends | Required if messages reference PHI |

## Testing checklist

1. Trigger each notification type in a non-prod environment; confirm the ESP
   dashboard shows `delivered`, not `bounced`/`failed`.
2. Confirm a missing/invalid required field returns a 4xx without attempting
   a send (fail before, not after, the API call).
3. Confirm the rate limiter actually blocks after N attempts, and that it's
   scoped correctly (IP for public, user ID for authenticated).
4. Confirm no PHI leaks into logs — check what `console.error` calls capture
   on a Resend failure; error details should not echo the message body back
   into logs.
5. Read the actual email as a recipient would — confirm no raw template
   placeholders, no PHI beyond what was explicitly approved, and a working
   reply-to/contact path if the recipient needs to respond.
