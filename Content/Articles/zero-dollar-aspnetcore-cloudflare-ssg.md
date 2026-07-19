---
title: "Zero-Cost ASP.NET Core: Building a Markdown Blog on Cloudflare Pages"
description: "How to combine the power of ASP.NET Core with Python static site generation to host a fully-featured, markdown-driven blog on Cloudflare for $0/month."
datePublished: "2026-07-19T22:30:00Z"
dateModified: "2026-07-19T22:30:00Z"
isDraft: false
headerImageUrl: "https://www.rodneyachery.com/assets/images/rodney-chery-social-card.webp"
focusKeyword: "ASP.NET Core Cloudflare SSG"
tags: "C#, ASP.NET Core, Cloudflare, Architecture, Markdown"
---

# Introduction

When building a professional portfolio and tech blog, engineers often face a frustrating dilemma. 

On one hand, you want the robust architecture, strong typing, and tooling of **ASP.NET Core**. You want a system capable of injecting dynamic SEO metadata, generating precise XML sitemaps, and parsing Markdown seamlessly. 

On the other hand, traditional ASP.NET Core hosting implies running a dedicated server 24/7. To get this level of dynamic architecture, you would normally have to pay a provider like Microsoft Azure $20-$50/month.

In this article, I'll show you how we engineered a solution that gives you the best of both worlds: the full power of a C# backend on your local machine, permanently "frozen" into hyper-fast static files deployed globally on **Cloudflare Pages for exactly $0/month.**

## A Free, Fast Content Marketing Engine

I wanted a complete Technology Articles system without the bloat of a traditional database or a heavy CMS like WordPress. The solution? A **Markdown-driven architecture**.

If you want to write a technical blog post (e.g., *"Securing ASP.NET Core APIs"*), you simply create a `.md` text file. 

Using the `Markdig` and `YamlDotNet` NuGet packages, the ASP.NET Core backend parses the YAML front-matter to extract SEO data (like the focus keyword, tags, and publish date) and converts the Markdown body into pristine, beautifully rendered HTML. 

```csharp
// Extracting front-matter and rendering HTML seamlessly
var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
article.ContentHtml = Markdown.ToHtml(markdown, pipeline);
```

The system then automatically:
1. Creates a dedicated, SEO-optimized web page.
2. Appends the new article to a dynamic `/Articles/feed.xml` RSS feed.
3. Automatically updates the `/sitemap.xml` so Google crawls it immediately.

## You Pay $0 for Hosting While Getting the Best of Both Worlds

The magic happens during deployment. Because Cloudflare Pages is a static host, it doesn't natively execute C# code. If we just deployed our `.cshtml` Razor Pages, the server wouldn't know how to render them.

Instead of paying for Azure, we built a **Python Static Site Generator (SSG) Script** that runs automatically inside our GitHub Actions CI/CD pipeline. 

When code is pushed to the `main` branch, the pipeline executes the following sequence:

1. **Boot the Backend:** It spins up the ASP.NET Core server in the background using `dotnet run`.
2. **Crawl the Sitemap:** A Python script queries the live `sitemap.xml` endpoint, extracting every public route and dynamically generated article URL.
3. **Freeze the Output:** The script visits every single URL, capturing the fully rendered HTML—complete with JSON-LD structured data and OpenGraph tags—and saves it to the `wwwroot/` folder.
4. **Deploy to Cloudflare:** The pipeline shuts down the C# server and pushes the resulting static folder to Cloudflare via Wrangler.

```python
# A simplified snippet of the deployment script
sitemap_req = urllib.request.urlopen("http://localhost:5000/sitemap.xml")
root = ET.fromstring(sitemap_req.read())

for url in extract_urls(root):
    local_path = map_to_folder(url)
    content = urllib.request.urlopen(url).read()
    
    with open(local_path, "wb") as f:
        f.write(content)
```

## Conclusion

This hybrid approach completely changes the economics of hosting .NET web applications. 

By utilizing ASP.NET Core exclusively as an intelligent build-time rendering engine, we decoupled our content from our hosting environment. The result is a platform that offers unparalleled developer experience, a flawless 100/100 Lighthouse SEO score, zero database maintenance, and global CDN delivery—all for free.
