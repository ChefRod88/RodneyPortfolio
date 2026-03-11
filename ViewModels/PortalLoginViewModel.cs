using RodneyPortfolio.Models;

namespace RodneyPortfolio.ViewModels;

public class PortalLoginViewModel
{
    public LoginInput Input { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
