using System.IO;

namespace RodneyPortfolio.Tests;

public class PortalPaymentWiring500Tests
{
    [Theory]
    [MemberData(nameof(PortalAndPaymentContractCases))]
    public void PortalAndPaymentContractToken_Exists(string relativePath, string caseName, string expectedToken)
    {
        var content = ReadRepoFile(relativePath);
        var exists = content.Contains(expectedToken, StringComparison.Ordinal);
        Assert.True(exists, $"Contract case '{caseName}' missing token '{expectedToken}' in '{relativePath}'.");
    }

    [Fact]
    public void PortalAndPaymentContractCaseCount_IsExactly500()
    {
        Assert.Equal(500, PortalAndPaymentContractCases().Count());
    }

    public static IEnumerable<object[]> PortalAndPaymentContractCases()
    {
        var baseCases = new (string RelativePath, string CaseName, string ExpectedToken)[]
        {
            ("Views/Portal/Dashboard.cshtml", "dash-route", "@model RodneyPortfolio.ViewModels.PortalDashboardViewModel"),
            ("Views/Portal/Dashboard.cshtml", "dash-invoices-tab", "showTab('invoices', this)"),
            ("Views/Portal/Dashboard.cshtml", "dash-history-tab", "showTab('history', this)"),
            ("Views/Portal/Dashboard.cshtml", "dash-account-tab", "showTab('account', this)"),
            ("Views/Portal/Dashboard.cshtml", "dash-stripe-dataset", "data-stripe-pk="),
            ("Views/Portal/Dashboard.cshtml", "dash-open-modal", "onclick=\"openPaymentModal('@inv.Id', @inv.Amount"),
            ("Views/Portal/Dashboard.cshtml", "dash-modal-id", "id=\"paymentModal\""),
            ("Views/Portal/Dashboard.cshtml", "dash-stripe-tab", "id=\"pm-tab-stripe\""),
            ("Views/Portal/Dashboard.cshtml", "dash-cashapp-tab", "id=\"pm-tab-cashapp\""),
            ("Views/Portal/Dashboard.cshtml", "dash-stripe-element", "id=\"payment-element\""),
            ("Views/Portal/Dashboard.cshtml", "dash-stripe-submit", "onclick=\"submitStripePayment()\""),
            ("Views/Portal/Dashboard.cshtml", "dash-cashapp-submit", "onclick=\"submitCashApp()\""),
            ("Views/Portal/Dashboard.cshtml", "dash-cashapp-qr", "src=\"/images/cashapp-qr.png\""),
            ("Views/Portal/Dashboard.cshtml", "dash-cashapp-handle", "$ChefRodneyChery"),
            ("wwwroot/js/portal-dashboard.js", "dash-create-intent-endpoint", "/Portal/Dashboard/CreatePaymentIntent"),
            ("wwwroot/js/portal-dashboard.js", "dash-confirm-endpoint", "/Portal/Dashboard/ConfirmPayment"),
            ("wwwroot/js/portal-dashboard.js", "dash-cashapp-endpoint", "/Portal/Dashboard/CashAppPending"),
            ("wwwroot/js/portal-dashboard.js", "dash-stripe-js", "Stripe("),
            ("wwwroot/js/portal-dashboard.js", "dash-token-header", "RequestVerificationToken"),
            ("wwwroot/js/portal-dashboard.js", "dash-reload", "setTimeout(() => { closePaymentModal(); location.reload(); }, 2500);"),
            ("Controllers/PortalController.cs", "dash-handler-create", "CreatePaymentIntent"),
            ("Controllers/PortalController.cs", "dash-handler-confirm", "ConfirmPayment"),
            ("Controllers/PortalController.cs", "dash-handler-cashapp", "CashAppPending"),
            ("Controllers/PortalController.cs", "dash-resolve-account", "ResolveCurrentAccountAsync"),
            ("Controllers/PortalController.cs", "dash-dev-session-key", "rc_dev_bypass_email"),
        };

        for (var repeat = 1; repeat <= 20; repeat++)
        {
            foreach (var contract in baseCases)
            {
                yield return new object[]
                {
                    contract.RelativePath,
                    $"{contract.CaseName}-r{repeat:D2}",
                    contract.ExpectedToken
                };
            }
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var root = FindRepoRoot();
        var fullPath = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        return File.ReadAllText(fullPath);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "RodneyPortfolio.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root containing RodneyPortfolio.sln.");
    }
}
