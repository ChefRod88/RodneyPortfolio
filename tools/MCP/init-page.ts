// tools/mcp/init-page.ts
// This runs when the MCP server creates a page.
// We use it to automatically navigate to your local app and wait for it to load.

export default async ({ page }: { page: any }) => {
    // Change this port to whatever your app uses.
    const url = "http://localhost:5076";
  
    // Go to your local app
    await page.goto(url, { waitUntil: "networkidle" });
  
    // Optional: small pause so you visually see it settle
    await page.waitForTimeout(500);
  };
  