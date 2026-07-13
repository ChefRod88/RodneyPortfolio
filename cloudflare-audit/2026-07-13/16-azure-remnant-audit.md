# Azure Remnant Audit

## Obsolete Configuration and Code
* **DNS Records**:
  * `rodney-portfolio.azurewebsites.net` (CNAME)
  * `newbethel.azurewebsites.net` (CNAME)
* **GitHub Actions Workflows**: None (Successfully removed Azure workflow files and replaced with Cloudflare).
* **Source Code**:
  * `appsettings.json` contains Azure SQL and connection options.
  * `Program.cs` contains Azure Key Vault, Azure SQL, and connection options.
  * These are historical and can be ignored since the application is now hosted as static assets on Cloudflare.