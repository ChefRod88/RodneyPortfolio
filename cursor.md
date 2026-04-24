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

## Plan → approve → implement

**Triggers:** You use **`?`** in a message *and* the request involves **implementation** in the repo, **or** you say **plan** (e.g. “show me the plan first”). The agent will **show a plan** before changing code.

**Approval:** You type **`apv`** when you **approve** that plan. The agent only **implements** after seeing `apv` (unless you explicitly skip planning, e.g. “no plan, just do it”).

*Defined in `.cursor/rules/plan-then-approve.mdc`.*

---

## ACP (Add → Commit → Push)

**Trigger:** When you type **`acp`** in chat, the agent will **`git add -A`**, **commit** with a concise message, and **push** the current branch (e.g. to `origin/main`). It does not run on every save—only when you say `acp` or clearly ask to add/commit/push in the same way.

*Defined in `.cursor/rules/acp-git.mdc`.*

---

*Also enforced via `.cursor/rules/css-js-folders.mdc` so the agent applies this in every session.*
