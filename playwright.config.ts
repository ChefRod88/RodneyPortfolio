// playwright.config.ts
import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./tests",
  use: {
    baseURL: "http://localhost:5076", // CHANGE to your port
    trace: "on-first-retry"
  },
  projects: [
    {
      name: "Mobile Chrome - iPhone 15",
      use: {
        ...devices["iPhone 15"],
        channel: "chrome"
      }
    }
  ]
});
