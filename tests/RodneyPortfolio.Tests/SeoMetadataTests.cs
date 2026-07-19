using RodneyPortfolio.Models;
using Xunit;

namespace RodneyPortfolio.Tests;

public class SeoMetadataTests
{
    [Fact]
    public void SeoMetadata_DefaultValues_AreSetCorrectly()
    {
        // Arrange
        var metadata = new SeoMetadata();

        // Act & Assert
        Assert.Equal("Rodney Chery - Full Stack Software Engineer", metadata.Title);
        Assert.Equal("index, follow", metadata.Robots);
        Assert.Equal("website", metadata.OpenGraphType);
        Assert.Equal("Rodney Chery", metadata.Author);
    }
}
