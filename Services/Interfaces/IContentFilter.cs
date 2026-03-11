namespace RodneyPortfolio.Services;

public interface IContentFilter
{
    bool IsBlocked(string? message);
}
