namespace RodneyPortfolio.Services;

public interface IInputValidator
{
    bool IsValid(string? message);
    string? GetValidationError(string? message);
}
