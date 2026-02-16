# Church Website Codebase Index

Quick reference for locating and editing parts of the site.

---

## Quick Lookup: "I want to change..."

| What | File(s) |
|------|---------|
| Nav links (Home, About, Next Steps, Media, Request Prayer) | `Pages/Shared/_Layout.cshtml` |
| Nav styling (colors, transparent over hero) | `wwwroot/css/site.css` (lines 50-96) |
| Hero image/headline | `appsettings.json` (HeroImageUrl, HeroHeadline), `Pages/Index.cshtml` |
| Footer (contact, social links) | `Pages/Shared/_Layout.cshtml`, `wwwroot/css/site.css` |
| Church name, address, service times | `appsettings.json`, `Models/ChurchSettings.cs` |
| Sermons list/data | `Services/SermonService.cs`, `Models/Sermon.cs`, `Pages/Sermons/*` |
| Events list/data | `Services/EventService.cs`, `Models/ChurchEvent` in `Models/Event.cs`, `Pages/Events/*` |
| Groups list/data | `Services/GroupService.cs`, `Models/Group.cs`, `Pages/Groups/*` |
| Prayer request form | `Pages/Prayer.cshtml`, `Pages/Prayer.cshtml.cs` |
| Live stream embed | `Pages/Live.cshtml`, `appsettings.json` (LiveStreamUrl) |
| Home page sections | `Pages/Index.cshtml` |
| Transparent nav on scroll | `wwwroot/js/site.js` |
| Smooth scroll, focus styles | `wwwroot/js/site.js`, `wwwroot/css/site.css` |
| Church theme colors | `wwwroot/css/site.css` (`:root` variables) |

---

## Project Structure

```
ChurchWebsite/
├── appsettings.json          # Church config (name, hero, contact, social)
├── Program.cs                # App startup, DI, services, routing
├── Models/
│   ├── ChurchSettings.cs     # Config model for appsettings Church section
│   ├── Event.cs              # ChurchEvent model
│   ├── Group.cs              # Group model
│   └── Sermon.cs             # Sermon model
├── Services/
│   ├── EventService.cs       # In-memory events data
│   ├── GroupService.cs       # In-memory groups data
│   └── SermonService.cs       # In-memory sermons data
├── Pages/
│   ├── _ViewStart.cshtml     # Sets default layout for all pages
│   ├── _ViewImports.cshtml   # Global usings, tag helpers
│   ├── Shared/
│   │   ├── _Layout.cshtml    # Shared layout: nav, footer, scripts
│   │   └── _ValidationScriptsPartial.cshtml
│   ├── Index.cshtml          # Home page
│   ├── About.cshtml          # About page
│   ├── Prayer.cshtml         # Request Prayer form
│   ├── Live.cshtml           # Watch Live embed
│   ├── Events/               # Events list + details
│   ├── Groups/               # Groups list + details
│   └── Sermons/              # Sermons list + details
└── wwwroot/
    ├── css/site.css          # Custom styles (nav, hero, footer, etc.)
    ├── js/
    │   ├── site.js           # Smooth scroll, transparent nav on scroll
    │   └── hero-banner.js    # Optional: scrolling banner (if used)
    └── lib/                  # Third-party (Bootstrap, jQuery) - do not modify
```

---

## Config

| Setting | File | Purpose |
|---------|------|---------|
| Church.Name, Tagline | appsettings.json | Church name in nav, footer, titles |
| HeroImageUrl | appsettings.json | Hero background image path |
| HeroHeadline | appsettings.json | Main headline on home hero |
| ServiceTimes | appsettings.json | Displayed on About or home |
| Address, Phone, Email | appsettings.json | Footer contact |
| LiveStreamUrl | appsettings.json | YouTube embed URL for Live page |
| SocialMedia | appsettings.json | Facebook, YouTube, Instagram links |
