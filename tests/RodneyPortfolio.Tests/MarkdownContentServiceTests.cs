using Moq;
using Microsoft.AspNetCore.Hosting;
using RodneyPortfolio.Services;
using Xunit;

namespace RodneyPortfolio.Tests;

public class MarkdownContentServiceTests
{
    [Fact]
    public async Task GetAllArticlesAsync_ReturnsEmptyList_WhenDirectoryDoesNotExist()
    {
        // Arrange
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.ContentRootPath).Returns(Directory.GetCurrentDirectory());
        var service = new MarkdownContentService(mockEnv.Object);

        // Act
        var result = await service.GetAllArticlesAsync("NonExistentFolder");

        // Assert
        Assert.Empty(result);
    }
}
