# Migration: Microsoft Azure to Cloudflare

This document records the migration of the RodneyPortfolio website and application from Microsoft Azure to Cloudflare, completed in July 2026.

## Historical Azure Architecture
* **Web App Hosting**: Azure App Service (Windows-based or Linux-based, West US 3).
* **Database**: Azure SQL Database (`database.windows.net`) used for the Client Portal and invoice management.
* **Authentication**: Session-based cookie auth with email-based One-Time Passwords (OTP).
* **CI/CD Pipeline**: GitHub Actions utilizing `azure/webapps-deploy@v3` and `Azure/appservice-settings@v1` for environment settings.

## New Cloudflare Architecture
* **Web App Hosting**: Cloudflare Pages serving compiled static assets from `./wwwroot`.
* **Database**: Dynamic database client database is offline/disabled for the static version; client database requests use fallbacks.
* **DNS and SSL/TLS**: Fully authoritative on Cloudflare with Full (Strict) SSL and HTTPS enforcement.
* **CI/CD Pipeline**: GitHub Actions utilizing `cloudflare/wrangler-action@v3` targeting the `main` branch.

## Key Changes
1. **Static compilation**: Dynamically compiled .NET Razor Pages were scraped into static HTML files (like `index.html`, `Agreement/index.html`, etc.) and placed in `wwwroot/` so that Cloudflare Pages can serve them instantly from edge nodes.
2. **Version hashes cleanup**: Removed C# `MapStaticAssets` cache-busting hashes (e.g. `.o1o13a6vjx.js` -> `.js`) from the static HTML files to prevent 404 errors on Cloudflare.
3. **Obsolete Azure references removed**: Cleaned up the build pipeline, configuration connections, and main documentation.
