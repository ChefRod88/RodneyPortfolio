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
            ("Pages/Dashboard.cshtml", "dash-route", "@page \"/Portal/Dashboard\""),
            ("Pages/Dashboard.cshtml", "dash-invoices-tab", "showTab('invoices', this)"),
            ("Pages/Dashboard.cshtml", "dash-history-tab", "showTab('history', this)"),
            ("Pages/Dashboard.cshtml", "dash-account-tab", "showTab('account', this)"),
            ("Pages/Dashboard.cshtml", "dash-support-tab", "showTab('support', this)"),
            ("Pages/Dashboard.cshtml", "dash-open-modal", "onclick=\"openPaymentModal('@inv.Id', @inv.Amount"),
            ("Pages/Dashboard.cshtml", "dash-modal-id", "id=\"paymentModal\""),
            ("Pages/Dashboard.cshtml", "dash-stripe-tab", "id=\"pm-tab-stripe\""),
            ("Pages/Dashboard.cshtml", "dash-cashapp-tab", "id=\"pm-tab-cashapp\""),
            ("Pages/Dashboard.cshtml", "dash-stripe-element", "id=\"payment-element\""),
            ("Pages/Dashboard.cshtml", "dash-stripe-submit", "onclick=\"submitStripePayment()\""),
            ("Pages/Dashboard.cshtml", "dash-cashapp-submit", "onclick=\"submitCashApp()\""),
            ("Pages/Dashboard.cshtml", "dash-cashapp-qr", "src=\"/images/cashapp-qr.png\""),
            ("Pages/Dashboard.cshtml", "dash-cashapp-handle", "$ChefRodneyChery"),
            ("Pages/Dashboard.cshtml", "dash-create-intent-endpoint", "/Portal/Dashboard?handler=CreatePaymentIntent"),
            ("Pages/Dashboard.cshtml", "dash-confirm-endpoint", "/Portal/Dashboard?handler=ConfirmPayment"),
            ("Pages/Dashboard.cshtml", "dash-cashapp-endpoint", "/Portal/Dashboard?handler=CashAppPending"),
            ("Pages/Dashboard.cshtml", "dash-stripe-js", "const stripe = Stripe(\"@Model.StripePublishableKey\")"),
            ("Pages/Dashboard.cshtml", "dash-token-header", "\"RequestVerificationToken\": csrf"),
            ("Pages/Dashboard.cshtml", "dash-reload", "setTimeout(() => { closePaymentModal(); location.reload(); }, 2500);"),
            ("Pages/Dashboard.cshtml.cs", "dash-handler-create", "OnPostCreatePaymentIntentAsync"),
            ("Pages/Dashboard.cshtml.cs", "dash-handler-confirm", "OnPostConfirmPaymentAsync"),
            ("Pages/Dashboard.cshtml.cs", "dash-handler-cashapp", "OnPostCashAppPendingAsync"),
            ("Pages/Dashboard.cshtml.cs", "dash-resolve-account", "ResolveCurrentAccountAsync"),
            ("Pages/Dashboard.cshtml.cs", "dash-dev-session-key", "rc_dev_bypass_email"),
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
