# Resend Email Setup (Cloudflare Worker)

Instructions for an LLM/agent setting up or troubleshooting transactional email
for this project. Follow these steps in order; don't skip the "why" — the
default sender restrictions below are the most common cause of "the form
submitted successfully but no email arrived."

## Purpose

`worker/index.js` handles the Quote form (`wwwroot/index.html`) and Support
form (`wwwroot/Support/index.html`) POSTs — the old ASP.NET/SMTP backend has
been fully retired; see `docs/migration/azure-to-cloudflare.md`. Both handlers call `sendEmail()` in
`worker/index.js`, which posts to the Resend API (`api.resend.com/emails`).
Resend is the only thing standing between a form submission and Rodney
actually receiving it. If it's misconfigured, `sendEmail()` throws and the
handler returns a 500 with "Unable to submit right now."

## Required configuration

`sendEmail()` reads three `env` values:

| Name | Type | Set via | Current value |
|---|---|---|---|
| `RESEND_API_KEY` | secret | `wrangler secret put` | not set — must be provisioned |
| `TO_EMAIL` | var or secret | `wrangler.json` `vars` or `wrangler secret put` | not set — must be provisioned |
| `FROM_EMAIL` | var | `wrangler.json` `vars` | `onboarding@resend.dev` (already set) |

`TURNSTILE_SECRET_KEY` (secret) is a sibling requirement for the same forms —
without it `verifyTurnstile()` fails closed and every submission is rejected
before `sendEmail()` is ever reached. Provision it in the same pass; see
`.dev.vars` for the local dev value (Cloudflare's test key).

## Step 1: Get a Resend API key

1. Sign in (or sign up) at Resend and open the dashboard.
2. Go to **API Keys** → **Create API Key**.
3. Name it something identifiable, e.g. `rodney-portfolio-worker`.
4. Set permission to **Sending access** only (not Full access) — this worker
   only ever calls the send-email endpoint.
5. If Resend offers domain-scoping for the key, restrict it to the sending
   domain from Step 2 once that domain exists.
6. Copy the key immediately (`re_...`) — Resend only shows it once.

## Step 2: Decide the FROM domain

`FROM_EMAIL` is currently `onboarding@resend.dev`, Resend's shared test
sender. **This only delivers to the email address that owns the Resend
account** — it will silently fail (or bounce) for anyone else, including
`rodney@globalrcdev.com` if that's not the account's own login email. This is
fine for a first smoke test but is not production-ready.

To send to arbitrary recipients (any visitor's email, forwarded to
`rodney@globalrcdev.com`), verify a real sending domain:

1. In Resend, go to **Domains** → **Add Domain**. Use a domain Rodney
   controls (e.g. a subdomain like `mail.rodneyachery.com` or
   `mail.globalrcdev.com` to keep it isolated from the main site's DNS).
2. Resend will show DNS records to add: typically an **MX** record, a
   **TXT** record for SPF, and a **TXT/CNAME** set for DKIM.
3. Since Cloudflare is authoritative DNS for this project (see
   `docs/migration/azure-to-cloudflare.md`), add these records in the
   Cloudflare dashboard for that domain/zone — **not proxied** (DNS-only /
   grey-cloud) for MX and the verification TXT/CNAME records.
4. Wait for Resend to show the domain as **Verified** (DNS propagation can
   take a few minutes to a few hours).
5. Update `FROM_EMAIL` in `wrangler.json` (`vars.FROM_EMAIL`) to an address
   on the verified domain, e.g. `no-reply@mail.rodneyachery.com`.

If Rodney explicitly wants to keep using `onboarding@resend.dev` for now
(e.g. low volume, testing phase), leave `FROM_EMAIL` as-is but flag clearly
that only the Resend account owner's own inbox will receive mail.

## Step 3: Set TO_EMAIL

This is where form submissions get delivered — currently
`rodney@globalrcdev.com`, set as a plain var in `wrangler.json` (not a
secret, since it's not sensitive data):

```json
"vars": {
  "FROM_EMAIL": "onboarding@resend.dev",
  "TO_EMAIL": "rodney@globalrcdev.com"
}
```

Only use `wrangler secret put TO_EMAIL` instead if Rodney explicitly wants it
kept out of the repo.

## Step 4: Provision secrets

**Production** (requires Cloudflare API auth already configured for this
account — see `docs/cloudflare-deployment.md`):

```bash
npx wrangler secret put RESEND_API_KEY
# paste the re_... key when prompted

npx wrangler secret put TURNSTILE_SECRET_KEY
# paste the Turnstile secret key (from the Cloudflare Turnstile dashboard,
# matching the site key already embedded in the wwwroot HTML forms)

# only if TO_EMAIL was kept as a secret instead of a var:
npx wrangler secret put TO_EMAIL
```

Verify what's set (values are never shown, only names):

```bash
npx wrangler secret list
```

**Local dev** — add to `.dev.vars` (already gitignored, do not commit):

```
TURNSTILE_SECRET_KEY=1x0000000000000000000000000000000AA
RESEND_API_KEY=re_...
TO_EMAIL=rodney@globalrcdev.com
```

Run `npx wrangler dev` and it will load `.dev.vars` automatically alongside
`wrangler.json`'s `vars`.

## Step 5: Verify it actually works

1. `npx wrangler dev`, open the Quote or Support form, submit with a real
   Turnstile pass (Cloudflare's test site key auto-passes in dev).
2. Confirm a 200 JSON response (`{ "ok": true }`), not a 500.
3. Check the Resend dashboard → **Emails** (or **Logs**) tab for the send
   event and its delivery status.
4. If email doesn't arrive: check the Resend log first — status
   `delivered` means it's a mailbox/spam-filter issue, not a config issue.
   `bounced`/`failed` with a domain-related error usually means Step 2's DNS
   records aren't verified yet, or `FROM_EMAIL` doesn't match the verified
   domain.
5. After confirming locally, deploy (`npx wrangler deploy` or via the
   `main_rodney-portfolio.yml` GitHub Actions workflow) and repeat the
   submission test against the live site.

## Common failure modes

- **500 "Unable to submit right now"** → `RESEND_API_KEY` or `TO_EMAIL`
  missing in the target environment (`sendEmail()` throws before calling the
  API — see `worker/index.js`).
- **200 response but no email** → `FROM_EMAIL` is still `onboarding@resend.dev`
  and `TO_EMAIL` isn't the Resend account owner's address (Step 2).
- **400 "Please complete the CAPTCHA"** → `TURNSTILE_SECRET_KEY` missing or
  wrong, unrelated to Resend but blocks reaching `sendEmail()` at all.
- **429 "Too many requests"** → expected behavior from `QUOTE_RATE_LIMITER` /
  `SUPPORT_RATE_LIMITER` (5 requests/60s per IP); not a Resend issue.
- **Everything above checks out (secrets set, `wrangler deploy` succeeds) but
  the live domain still doesn't work** → `wrangler.json`'s `"name"` may not
  match the Worker script your custom domain is actually bound to. Cloudflare
  accounts can silently accumulate multiple similarly-named scripts (e.g.
  `rodney-portfolio` vs `rodneyportfolio`), and secrets/deploys only apply to
  whichever `"name"` is in `wrangler.json` at deploy time — not necessarily
  the one live traffic hits. Verify with:
  `curl -s https://api.cloudflare.com/client/v4/accounts/{account_id}/workers/domains -H "Authorization: Bearer $CLOUDFLARE_API_TOKEN"`
  and confirm the `service` field for your hostname matches `wrangler.json`'s
  `"name"` exactly. This exact mismatch cost a full debugging session once
  already — check it early next time.
