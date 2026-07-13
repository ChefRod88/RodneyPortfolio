# Cloudflare Deployment Documentation

This document explains the deployment workflow and procedures for the RodneyPortfolio project.

## CI/CD Pipeline
The deployment is fully automated via GitHub Actions:
* **Workflow File**: `.github/workflows/main_rodney-portfolio.yml`
* **Trigger**: Triggers on push to `main` or manual `workflow_dispatch`.
* **Deployment Tool**: `cloudflare/wrangler-action@v3` with `wranglerVersion: 'latest'`.
* **Authentication**: Uses `CLOUDFLARE_API_TOKEN` stored as a GitHub Repository Secret.

## Local Deployment Validation
Before pushing changes, the static build outputs can be previewed locally using Wrangler:
```bash
# Start Wrangler development server for static assets
npx wrangler dev
```

## Production Release Flow
1. Run local tests and verify C# builds.
2. Compile/export any new Razor Pages to their static counterparts under `wwwroot/` if required.
3. Clean up cache-busting hashes from HTML files using the cleanup pattern `\.[a-z0-9]{10}\.(js|css|svg|json|png|jpg|jpeg|gif|ico)`.
4. Commit changes and push to the `main` branch.
5. GitHub Action triggers the build/publish verification and deploys the `wwwroot` directory to Cloudflare Pages.
