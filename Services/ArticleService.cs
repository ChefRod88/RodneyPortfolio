using Markdig;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RodneyPortfolio.Models
{
    public class Article
    {
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTimeOffset DatePublished { get; set; }
        public DateTimeOffset? DateModified { get; set; }
        public List<string> Tags { get; set; } = new();
        public bool IsDraft { get; set; }
        public string ContentHtml { get; set; } = string.Empty;
        public string HeaderImageUrl { get; set; } = string.Empty;
        public string FocusKeyword { get; set; } = string.Empty;
    }
}

namespace RodneyPortfolio.Services
{
    public interface IArticleService
    {
        Task<List<Models.Article>> GetAllArticlesAsync(bool includeDrafts = false);
        Task<Models.Article?> GetArticleBySlugAsync(string slug);
    }

    public class ArticleService : IArticleService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ArticleService> _logger;
        private readonly IDeserializer _yamlDeserializer;

        public ArticleService(IWebHostEnvironment env, ILogger<ArticleService> logger)
        {
            _env = env;
            _logger = logger;
            _yamlDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
        }

        public async Task<List<Models.Article>> GetAllArticlesAsync(bool includeDrafts = false)
        {
            var articlesPath = Path.Combine(_env.ContentRootPath, "Content", "Articles");
            if (!Directory.Exists(articlesPath)) return new List<Models.Article>();

            var files = Directory.GetFiles(articlesPath, "*.md");
            var articles = new List<Models.Article>();

            foreach (var file in files)
            {
                var article = await ParseArticleFileAsync(file);
                if (article != null)
                {
                    if (includeDrafts || !article.IsDraft)
                    {
                        articles.Add(article);
                    }
                }
            }

            return articles.OrderByDescending(a => a.DatePublished).ToList();
        }

        public async Task<Models.Article?> GetArticleBySlugAsync(string slug)
        {
            var articlesPath = Path.Combine(_env.ContentRootPath, "Content", "Articles");
            var filePath = Path.Combine(articlesPath, $"{slug}.md");
            
            if (!File.Exists(filePath)) return null;

            return await ParseArticleFileAsync(filePath);
        }

        private async Task<Models.Article?> ParseArticleFileAsync(string filePath)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                if (!content.StartsWith("---")) return null;

                var endIndex = content.IndexOf("---", 3);
                if (endIndex == -1) return null;

                var yaml = content.Substring(3, endIndex - 3).Trim();
                var markdown = content.Substring(endIndex + 3).Trim();

                var frontMatter = _yamlDeserializer.Deserialize<Dictionary<string, string>>(yaml);

                var article = new Models.Article
                {
                    Slug = Path.GetFileNameWithoutExtension(filePath),
                    Title = frontMatter.GetValueOrDefault("title", "Untitled"),
                    Description = frontMatter.GetValueOrDefault("description", ""),
                    DatePublished = DateTimeOffset.TryParse(frontMatter.GetValueOrDefault("datePublished"), out var dp) ? dp : DateTimeOffset.UtcNow,
                    DateModified = DateTimeOffset.TryParse(frontMatter.GetValueOrDefault("dateModified"), out var dm) ? dm : null,
                    IsDraft = bool.TryParse(frontMatter.GetValueOrDefault("isDraft"), out var draft) && draft,
                    HeaderImageUrl = frontMatter.GetValueOrDefault("headerImageUrl", ""),
                    FocusKeyword = frontMatter.GetValueOrDefault("focusKeyword", "")
                };

                var tagsString = frontMatter.GetValueOrDefault("tags", "");
                if (!string.IsNullOrWhiteSpace(tagsString))
                {
                    article.Tags = tagsString.Split(',').Select(t => t.Trim()).ToList();
                }

                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
                article.ContentHtml = Markdown.ToHtml(markdown, pipeline);

                return article;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse article {FilePath}", filePath);
                return null;
            }
        }
    }
}
