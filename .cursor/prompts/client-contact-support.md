# Add Contact Support footer link (client sites)

Use this prompt when adding a support entry point to a site Rodney built. The support form lives on the portfolio — do not embed a form on the client site.

## Goal

Add a **Contact Support** link in the site footer (or equivalent global footer partial) that opens:

`https://www.rodneyachery.com/#support`

That URL loads the portfolio home, then redirects to the public support page where the user can report a problem. Rodney receives an email when the form is submitted.

## Rules

1. **Link only** — no embedded iframe, no duplicate support form on the client site.
2. **Canonical URL** — always use `https://www.rodneyachery.com/#support` (with `www`).
3. **Match existing footer** — reuse the same markup pattern, CSS classes, and list structure as other footer links.
4. **Single source** — if the footer is a shared partial/layout, update it once.
5. **Minimal diff** — do not refactor unrelated footer or layout code.

## Optional attributes

- `target="_blank"` and `rel="noopener noreferrer"` if other external footer links use a new tab; otherwise same-tab is fine.

## Examples

### ASP.NET Core Razor (layout/footer partial)

```html
<a href="https://www.rodneyachery.com/#support" rel="noopener noreferrer">Contact Support</a>
```

### Static HTML

```html
<a href="https://www.rodneyachery.com/#support">Contact Support</a>
```

### React (footer component)

```jsx
<a href="https://www.rodneyachery.com/#support" rel="noopener noreferrer">
  Contact Support
</a>
```

## Verification

- Footer shows **Contact Support** (or agreed label) alongside existing links.
- Clicking the link opens `https://www.rodneyachery.com/#support` and lands on the **Report a Problem** support form.
- No new backend or email code on the client repo.

## Prompt to paste in Cursor

```
Add a "Contact Support" link to the site footer that points to https://www.rodneyachery.com/#support.
Follow the existing footer link markup and styles. Update only the shared footer (partial/layout/component).
Do not add a support form on this site — only the outbound link. Use rel="noopener noreferrer" if opening in a new tab.
```
