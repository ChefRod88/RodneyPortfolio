# Cloudflare Architecture Documentation

This document describes the current architecture of the RodneyPortfolio website and application.

## Client-Side Delivery (Cloudflare Pages)
* The application runs as a static site hosted entirely on Cloudflare Pages.
* Assets and static HTML generated from the .NET backend are stored under the `wwwroot` directory.
* Direct page routes like `/Agreement`, `/Faq`, `/Privacy`, `/Projects`, and `/Support` map to static `index.html` files inside subfolders (e.g. `wwwroot/Agreement/index.html`) to support clean URLs.
* Cache-control and gzip/brotli compression are automatically handled at the Cloudflare Edge nodes.

## Security Configuration
* **SSL/TLS Mode**: Full (Strict).
* **HTTPS Redirection**: Enforced at the DNS edge level.
* **WAF (Web Application Firewall)**: Handled by Cloudflare's default rulesets to block bots and SQL injection/cross-site scripting attacks on the client.

## Routing configuration
* Defined in `wrangler.json`:
  ```json
  "assets": {
    "directory": "./wwwroot",
    "not_found_handling": "404-page",
    "html_handling": "auto-trailing-slash"
  }
  ```
