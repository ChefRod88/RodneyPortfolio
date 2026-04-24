# Cursor guidance (this repository)

## CSS and JavaScript file layout

**Rule:** New CSS and JavaScript must live in dedicated folders, not loose next to pages or at the root of a web/static tree.

| Asset type   | Location (this app)   | Notes |
|-------------|------------------------|--------|
| Project CSS | `wwwroot/css/`         | App and page styles |
| Project JS  | `wwwroot/js/`          | App behavior |
| Third-party | `wwwroot/lib/`         | e.g. Bootstrap, jQuery (keep as today) |
| Other static | `wwwroot/assets/`     | images, documents, etc. |

- Prefer linked files (`~/css/...`, `~/js/...`) over large inline `<style>` / `<script>` blocks in views—keep views thin.
- A root-level `sw.js` (service worker) is acceptable when required for PWA scope; everything else new goes under `css/` or `js/`.

The same idea applies to any future front-end subproject: each app’s styles under a `css/` (or `styles/`) directory and scripts under `js/` (or `scripts/`), not mixed with markup at the same directory level.

---

*Also enforced via `.cursor/rules/css-js-folders.mdc` so the agent applies this in every session.*
