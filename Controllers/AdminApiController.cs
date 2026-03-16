using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Controllers;

/// <summary>
/// Admin-only endpoint for AI-powered resume context expansion.
/// POST /api/admin/expand-resume
/// Requires X-Admin-Token header matching Admin:ApiToken config value.
/// </summary>
[ApiController]
[Route("api/admin")]
public class AdminApiController : ControllerBase
{
    private readonly IAnthropicClient _anthropicClient;
    private readonly IResumeContextLoader _resumeContextLoader;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly ILogger<AdminApiController> _logger;

    public AdminApiController(
        IAnthropicClient anthropicClient,
        IResumeContextLoader resumeContextLoader,
        IWebHostEnvironment env,
        IConfiguration config,
        ILogger<AdminApiController> logger)
    {
        _anthropicClient = anthropicClient;
        _resumeContextLoader = resumeContextLoader;
        _env = env;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Reads docs/Rodney_Chery_Resume.md, sends it to Claude to produce an expanded AI context document,
    /// writes the result to Data/ResumeContext.txt, and invalidates the in-memory cache.
    /// </summary>
    [HttpPost("expand-resume")]
    public async Task<IActionResult> ExpandResume(CancellationToken cancellationToken)
    {
        var expectedToken = _config["Admin:ApiToken"];
        if (!string.IsNullOrWhiteSpace(expectedToken))
        {
            var provided = Request.Headers["X-Admin-Token"].FirstOrDefault();
            if (!string.Equals(provided, expectedToken, StringComparison.Ordinal))
                return Unauthorized(new { error = "Invalid or missing X-Admin-Token header." });
        }

        var resumeMdPath = Path.Combine(_env.ContentRootPath, "docs", "Rodney_Chery_Resume.md");
        if (!System.IO.File.Exists(resumeMdPath))
            return NotFound(new { error = "Source resume not found at docs/Rodney_Chery_Resume.md." });

        var resumeMarkdown = await System.IO.File.ReadAllTextAsync(resumeMdPath, cancellationToken);

        var prompt = BuildExpansionPrompt(resumeMarkdown);
        var requestBody = new
        {
            model = _config["Anthropic:Model"] ?? "claude-sonnet-4-6",
            max_tokens = 8192,
            messages = new[] { new { role = "user", content = prompt } }
        };

        HttpResponseMessage response;
        try
        {
            response = await _anthropicClient.PostMessagesAsync(requestBody, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { error = ex.Message });
        }

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Anthropic expand-resume failed: {Status} {Body}", response.StatusCode, err);
            return StatusCode(502, new { error = "Anthropic API call failed.", detail = err });
        }

        var expandedContent = await ExtractTextContent(response, cancellationToken);
        if (string.IsNullOrWhiteSpace(expandedContent))
            return StatusCode(502, new { error = "Empty response from Claude." });

        var outputPath = Path.Combine(_env.ContentRootPath, "Data", "ResumeContext.txt");
        await System.IO.File.WriteAllTextAsync(outputPath, expandedContent, cancellationToken);

        _resumeContextLoader.InvalidateCache();

        _logger.LogInformation("ResumeContext.txt expanded by Claude. Bytes written: {Bytes}", expandedContent.Length);
        return Ok(new { message = "Resume context expanded successfully.", bytes = expandedContent.Length });
    }

    private static string BuildExpansionPrompt(string resumeMarkdown) => $@"You are helping Rodney Chery expand his resume into a rich AI context document used as a system prompt for an AI chatbot on his portfolio website.

The chatbot answers questions about Rodney from recruiters, engineers, and interviewers. The context document must give the AI enough depth to answer varied, specific questions naturally.

Using the resume below as your source of truth, produce an expanded plain-text context document with these sections:

IDENTITY — Full name, location, contact, links, current title, work philosophy.
POSITIONING — Primary role focus, secondary strengths, career narrative (kitchens-to-tech).
CORE TECHNOLOGY STACK — All languages, frameworks, tools, cloud platforms, testing tools.
WORK EXPERIENCE DETAIL — For each role: company, title, dates, responsibilities, specific achievements with metrics where available, tools used, impact.
PROJECT DEEP-DIVES — For each project: purpose, full tech stack, architecture decisions, key engineering challenges solved, deployment strategy, what makes it notable.
SOFT SKILLS AND DIFFERENTIATORS — Communication style, working under pressure, customer empathy, cross-functional collaboration, specific examples if derivable.
EDUCATION — Institution, degree, expected graduation, relevant coursework or focus areas.
CONTACT QUICK LOOKUP — Name, email, LinkedIn, GitHub, portfolio URL in a scannable format.

Rules:
- Use only facts from the resume. Do not invent employers, dates, credentials, or technologies not mentioned.
- Be narrative and specific — write full sentences, not bullet fragments.
- Expand abbreviations and acronyms on first use.
- Output plain text only. No markdown, no headers with # symbols, no code fences.
- Use ALL CAPS section headings followed by a line of dashes (---).
- Make the document as rich as the source material allows — the AI needs depth to answer varied questions.

Resume source:
---
{resumeMarkdown}
---

Output the expanded context document now:";

    private static async Task<string?> ExtractTextContent(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);

            var content = doc.RootElement.GetProperty("content");
            foreach (var block in content.EnumerateArray())
            {
                if (block.TryGetProperty("type", out var t) && t.GetString() == "text"
                    && block.TryGetProperty("text", out var txt))
                    return txt.GetString();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}
