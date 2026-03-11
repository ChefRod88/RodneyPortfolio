using RodneyPortfolio.Models;

namespace RodneyPortfolio.ViewModels;

public class PortalRegisterViewModel
{
    public RegisterInput Input { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
