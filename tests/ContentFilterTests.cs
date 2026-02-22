using RodneyPortfolio.Services;

namespace RodneyPortfolio.Tests;

public class ContentFilterTests
{
    [Fact]
    public void IsBlocked_ReturnsFalse_WhenMessageIsNull()
    {
        Assert.False(ContentFilter.IsBlocked(null));
    }

    [Fact]
    public void IsBlocked_ReturnsFalse_WhenMessageIsClean()
    {
        Assert.False(ContentFilter.IsBlocked("What's Rodney's experience?"));
        Assert.False(ContentFilter.IsBlocked("Tell me about his skills"));
        Assert.False(ContentFilter.IsBlocked("Hello"));
    }

    [Theory]
    [InlineData("what the fuck")]
    [InlineData("this is shit")]
    public void IsBlocked_ReturnsTrue_WhenMessageContainsBlockedTerms(string message)
    {
        Assert.True(ContentFilter.IsBlocked(message));
    }

    [Fact]
    public void IsBlocked_ReturnsFalse_WhenWordIsPartOfLargerWord()
    {
        // "ass" in "class" or "pass" should not trigger
        Assert.False(ContentFilter.IsBlocked("What class is he in?"));
        Assert.False(ContentFilter.IsBlocked("Please pass the message"));
    }
}
