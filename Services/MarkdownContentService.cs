using System.Text.RegularExpressions;
using Markdig;
using RodneyPortfolio.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RodneyPortfolio.Services;

public interface IMarkdownContentService
{
    Task<List<Article>> GetAllArticlesAsync(string folderPath);
    Task<Article?> GetArticleAsync(string folderPath, string slug);
}

public class MarkdownContentService : IMarkdownContentService
{
    private readonly IWebHostEnvironment _env;

    public MarkdownContentService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<List<Article>> GetAllArticlesAsync(string folderPath)
    {
        var contentPath = Path.Combine(_env.ContentRootPath, folderPath);
        if (!Directory.Exists(contentPath)) return new List<Article>();

        var files = Directory.GetFiles(contentPath, "*.md");
        var articles = new List<Article>();

        foreach (var file in files)
        {
            var slug = Path.GetFileNameWithoutExtension(file);
            var article = await GetArticleAsync(folderPath, slug);
            if (article != null)
            {
                articles.Add(article);
            }
        }

        return articles.OrderByDescending(a => a.PublishedDate).ToList();
    }

    public async Task<Article?> GetArticleAsync(string folderPath, string slug)
    {
        var filePath = Path.Combine(_env.ContentRootPath, folderPath, $"{slug}.md");
        if (!File.Exists(filePath)) return null;

        var content = await File.ReadAllTextAsync(filePath);
        return ParseMarkdown(content, slug);
    }

    private Article ParseMarkdown(string content, string slug)
    {
        var article = new Article { Slug = slug };
        var match = Regex.Match(content, @"^---\s*\n(.*?)\n---\s*\n(.*)$", RegexOptions.Singleline);
        
        var markdownContent = content;

        if (match.Success)
        {
            var yaml = match.Groups[1].Value;
            markdownContent = match.Groups[2].Value;

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var frontmatter = deserializer.Deserialize<Dictionary<string, string>>(yaml);
            
            if (frontmatter.TryGetValue("title", out var title)) article.Title = title;
            if (frontmatter.TryGetValue("description", out var desc)) article.Description = desc;
            if (frontmatter.TryGetValue("author", out var author)) article.Author = author;
            if (frontmatter.TryGetValue("date", out var date) && DateTime.TryParse(date, out var parsedDate)) article.PublishedDate = parsedDate;
            if (frontmatter.TryGetValue("image", out var img)) article.ImageUrl = img;
            if (frontmatter.TryGetValue("tags", out var tags)) article.Tags = tags.Split(',').Select(t => t.Trim()).ToList();
        }

        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        article.HtmlContent = Markdown.ToHtml(markdownContent, pipeline);
        
        // Calculate reading time
        int wordCount = Regex.Matches(markdownContent, @"\b\w+\b").Count;
        article.ReadingTimeMinutes = (int)Math.Ceiling(wordCount / 200.0);

        return article;
    }
}
