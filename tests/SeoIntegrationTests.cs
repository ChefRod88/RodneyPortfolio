using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using AngleSharp;
using AngleSharp.Html.Dom;

namespace RodneyPortfolio.Tests
{
    public class SeoIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public SeoIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // Don't follow redirects to check status codes
            });
        }

        private async Task<IHtmlDocument> GetDocumentAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            return (IHtmlDocument)await context.OpenAsync(req => req.Content(content));
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Projects")]
        [InlineData("/Faq")]
        [InlineData("/Privacy")]
        [InlineData("/Articles")]
        public async Task PublicPages_ShouldHave_CorrectSeoTags(string url)
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var document = await GetDocumentAsync(response);

            // 1. Exactly one Title tag
            var titles = document.QuerySelectorAll("title");
            Assert.Single(titles);
            Assert.False(string.IsNullOrWhiteSpace(titles[0].TextContent));

            // 2. Exactly one canonical link
            var canonicals = document.QuerySelectorAll("link[rel='canonical']");
            Assert.Single(canonicals);
            Assert.StartsWith("https://www.rodneyachery.com", canonicals[0].GetAttribute("href"));

            // 3. OpenGraph tags
            Assert.NotNull(document.QuerySelector("meta[property='og:title']"));
            Assert.NotNull(document.QuerySelector("meta[property='og:description']"));
            Assert.NotNull(document.QuerySelector("meta[property='og:image']"));
            Assert.NotNull(document.QuerySelector("meta[property='og:url']"));
            
            // 4. JSON-LD structured data (at least one)
            var jsonLdScripts = document.QuerySelectorAll("script[type='application/ld+json']");
            Assert.NotEmpty(jsonLdScripts);
        }

        [Theory]
        [InlineData("/Admin/Accounts")]
        [InlineData("/Portal/Invoices")]
        [InlineData("/Agreement")]
        [InlineData("/Support")]
        public async Task PrivatePages_ShouldHave_NoIndex(string url)
        {
            // Note: If authentication redirects occur, we need to handle that or test the redirect target.
            // For now, if they are accessible or redirect to login, we can check the login page or the page itself.
            var response = await _client.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var document = await GetDocumentAsync(response);
                var robots = document.QuerySelector("meta[name='robots']");
                Assert.NotNull(robots);
                Assert.Contains("noindex", robots.GetAttribute("content"));
            }
        }
        
        [Fact]
        public async Task SitemapAndRobots_ShouldBeAccessible()
        {
            var robotsResponse = await _client.GetAsync("/robots.txt");
            robotsResponse.EnsureSuccessStatusCode();
            Assert.Equal("text/plain", robotsResponse.Content.Headers.ContentType?.MediaType);

            var sitemapResponse = await _client.GetAsync("/sitemap.xml");
            sitemapResponse.EnsureSuccessStatusCode();
            Assert.Equal("application/xml", sitemapResponse.Content.Headers.ContentType?.MediaType);
        }
    }
}
