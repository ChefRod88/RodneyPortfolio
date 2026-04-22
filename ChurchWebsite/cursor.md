# Prime Medical Group — Cursor rules

These rules are enforced for this project.

## CSS & JavaScript placement

**All CSS must live in `wwwroot/css/` and all JavaScript must live in `wwwroot/js/`.**

- **CSS** → `wwwroot/css/booking.css` (booking/appointment flow), `wwwroot/css/styles.css` (global)
- **JS** → `wwwroot/js/booking.js` (booking/appointment flow), `wwwroot/js/main.js` (global)

**Never write `<style>` blocks or `<script>` blocks inside `.cshtml` files.**  
Inline `style=""` attributes on elements are acceptable only for one-off layout values (e.g. `margin-bottom:0`) that are not reused anywhere else.

When adding styles or scripts for a new page or feature:

1. Append to the appropriate existing file (`booking.css` / `booking.js` for anything under `/book`, otherwise `styles.css` / `main.js`).
2. Use scoped class prefixes (e.g. `.sp-` for self-pay, `.lien-` for lien) to avoid collisions.
3. The layout (`Pages/Shared/_Layout.cshtml`) controls which bundles load via `ViewData["BookingPage"]` — keep that pattern; do not add new `<link>` or `<script>` tags directly in page files.

## Git workflow

**Every time you make changes, follow this exact sequence:**

```bash
git status                        # see what changed
git add .                         # stage all changes
git commit -m "type: description" # commit with a clear message
git push origin cursor/development-environment-setup-7ed9  # push to GitHub
```

**Commit message format:**

- `feat: add services section to homepage`
- `fix: logo not displaying on mobile`
- `style: update hero colors to match brand`
- `chore: update build artifacts`

**Rules:**

- Always `git status` before staging so you know exactly what you're committing
- Never commit with a vague message like "changes", "stuff", or "fix"
- Commit after each meaningful change — do not batch up many unrelated changes into one commit
- Always push immediately after committing — never leave commits only on local
- Never force-push (`git push --force`) without explicit instruction — it can overwrite history and break the Azure deployment
- The active branch is `cursor/development-environment-setup-7ed9` — always push to this branch

## Task completion (after every task)

When you finish a task:

1. **Commit and push** your work on the working branch (`cursor/development-environment-setup-7ed9`) if the task was not already committed.
2. **Merge into `main`** (via local merge or GitHub pull request, keeping history clean).
3. **Sync both branches:** `main` and `cursor/development-environment-setup-7ed9` should both contain the same completed work — merge `main` into the feature branch (or rebase as team policy allows) and **push both** to `origin` so local and remote stay aligned.

**Goal:** No completed work left only on one branch; Azure and collaborators see updates on `main`, and the named feature branch is not behind `main`.
