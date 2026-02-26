using RodneyPortfolio.Services;

namespace RodneyPortfolio.Tests;

public class ContentFilterTests
{
    private readonly IContentFilter _filter = new ContentFilter();

    [Fact]
    public void IsBlocked_ReturnsFalse_WhenMessageIsNull()
    {
        Assert.False(_filter.IsBlocked(null));
    }

    [Fact]
    public void IsBlocked_ReturnsFalse_WhenMessageIsClean()
    {
        Assert.False(_filter.IsBlocked("What's Rodney's experience?"));
        Assert.False(_filter.IsBlocked("Tell me about his skills"));
        Assert.False(_filter.IsBlocked("Hello"));
    }

    [Theory]
    [InlineData("what the fuck")]
    [InlineData("this is shit")]
    public void IsBlocked_ReturnsTrue_WhenMessageContainsBlockedTerms(string message)
    {
        Assert.True(_filter.IsBlocked(message));
    }

    [Fact]
    public void IsBlocked_ReturnsFalse_WhenWordIsPartOfLargerWord()
    {
        // "ass" in "class" or "pass" should not trigger
        Assert.False(_filter.IsBlocked("What class is he in?"));
        Assert.False(_filter.IsBlocked("Please pass the message"));
    }
}
