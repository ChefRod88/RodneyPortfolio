// tests/navbar-links.spec.ts
import { test, expect } from "@playwright/test";

test("Navbar links work (mobile)", async ({ page }) => {
  // 1) Go to home page (ensure app is running on port in playwright.config.ts)
  await page.goto("/");

  // 2) On mobile, nav is collapsed behind hamburger - open it first
  await page.locator(".hamburger-icon").click();

  // 3) Portfolio uses hash anchors (#about, #experience, etc.) - same page, not separate routes
  const navLinks = [
    { name: "About", hash: "#about" },
    { name: "Experience", hash: "#experience" },
    { name: "Projects", hash: "#projects" },
    { name: "Contact", hash: "#contact" },
  ];

  for (const link of navLinks) {
    // Re-open hamburger before each click (menu closes after each link click)
    await page.locator(".hamburger-icon").click();
    await page.getByRole("link", { name: link.name, exact: true }).click();

    // URL should contain the hash (e.g. /#about or #about)
    await expect(page).toHaveURL(new RegExp(`${link.hash.replace("#", "\\#")}`));

    // Basic “page is alive” check (adjust to your UI):
    // This checks that something visible exists, not a blank/error screen.
    await expect(page.locator("body")).toBeVisible();
  }
});


