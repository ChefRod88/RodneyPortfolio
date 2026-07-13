# Security Audit

* **WAF Rules**: Default Cloudflare protection active.
* **Custom Security Rules**: Rate limiting applied via standard Cloudflare DDoS/Bot settings.
* **turnstile**: Not currently configured in the static HTML files.
* **Administrative Endpoints**: Static `/Admin/Login` folder exists, but it contains a static front-end page with no active backend auth handler. Since Azure hosting is removed, it is safe from database attacks but disabled.