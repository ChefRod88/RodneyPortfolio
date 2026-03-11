using System.IO;

namespace RodneyPortfolio.Tests;

/// <summary>
/// 20 unit tests verifying:
///   1. The payment modal scroll/overflow fix is in place (body lock, overlay scroll, container visible)
///   2. All critical payment modal UI contracts are intact after the CSS change
/// </summary>
public class PaymentModalScrollTests
{
    // ── helpers ─────────────────────────────────────────────────────────────
    private static string DashboardView => ReadRepoFile("Views/Portal/Dashboard.cshtml");
    private static string DashboardCode => ReadRepoFile("Controllers/PortalController.cs");

    // ════════════════════════════════════════════════════════════════════════
    // GROUP 1 — Overlay scroll fix
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Overlay_HasOverflowYAuto_SoModalContentIsScrollable()
    {
        Assert.Contains("overflow-y:auto", DashboardView, StringComparison.Ordinal);
    }

    [Fact]
    public void Overlay_UsesAlignItemsFlexStart_SoModalDoesNotGetClipped()
    {
        Assert.Contains("align-items:flex-start", DashboardView, StringComparison.Ordinal);
    }

    [Fact]
    public void Overlay_HasPadding_SoModalHasBreathingRoomAtTopAndBottom()
    {
        Assert.Contains("padding:2rem 0.5rem", DashboardView, StringComparison.Ordinal);
    }

    // ════════════════════════════════════════════════════════════════════════
    // GROUP 2 — Container overflow fix
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Container_DoesNotHaveOverflowHidden_WhichWasClippingStripeForm()
    {
        // overflow:hidden on .pm-container was the root cause — must not exist
        Assert.DoesNotContain(".pm-container { background:#0d1117; border:1px solid rgba(0,212,255,0.2); border-radius:16px; width:min(480px,95vw); overflow:hidden",
            DashboardView, StringComparison.Ordinal);
    }

    [Fact]
    public void Container_HasOverflowVisible_SoStripeElementsRenderFully()
    {
        Assert.Contains("overflow:visible", DashboardView, StringComparison.Ordinal);
    }

    [Fact]
    public void Container_HasFlexShrinkZero_SoItDoesNotCollapse()
    {
        Assert.Contains("flex-shrink:0", DashboardView, StringComparison.Ordinal);
    }

    [Fact]
    public void Container_HasMarginAuto_SoItCentersInsideScrollableOverlay()
    {
        Assert.Contains("margin:auto", DashboardView, StringComparison.Ordinal);
    }

    // ════════════════════════════════════════════════════════════════════════
    // GROUP 3 — Body scroll lock
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void OpenPaymentModal_LocksBodyScroll_SoPageDoesNotScrollBehindModal()
    {
        Assert.Contains("document.body.style.overflow = \"hidden\"", DashboardView, StringComparison.Ordinal);
    }

    [Fact]
    public void ClosePaymentModal_UnlocksBodyScroll_SoPageScrollsAgainAfterClose()
    {
        Assert.Contains("document.body.style.overflow = \"\"", DashboardView, StringComparison.Ordinal);
    }

    // ════════════════════════════════════════════════════════════════════════
    // GROUP 4 — Critical modal UI elements still present after CSS change
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Modal_ConfirmPaymentButton_IsPresent()
    {
        Assert.Contains("onclick=\"submitStripePayment()\"", DashboardView, StringComparison.Ordinal);
    }

    [Fact]
    public void Modal_StripePaymentElement_MountPointIsPresent()
    {
        Assert.Contains("id=\"payment-element\"", DashboardView, StringComparison.Ordinal);
    }

    [Fact]
    public void Modal_CancelButton_IsPresent()
    {
        Assert.Contains("onclick=\"closePaymentModal()\"", DashboardView, StringComparison.Ordinal);
    }

    [Fact]
    public void Modal_SecureBadge_IsPresent()
    {
        Assert.Contains("Secured by Stripe", DashboardView, StringComparison.Ordinal);
    }

    [Fact]
    public void Modal_ProcessingSpinner_IsPresent()
    {
        Assert.Contains("id=\"pm-processing\"", DashboardView, StringComparison.Ordinal);
    }

    [Fact]
    public void Modal_SuccessMessage_IsPresent()
    {
        Assert.Contains("id=\"pm-success\"", DashboardView, StringComparison.Ordinal);
    }

    [Fact]
    public void Modal_ErrorDisplay_IsPresent()
    {
        Assert.Contains("id=\"pm-stripe-error\"", DashboardView, StringComparison.Ordinal);
    }

    // ════════════════════════════════════════════════════════════════════════
    // GROUP 5 — Backend payment handlers still intact
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Backend_CreatePaymentIntentHandler_IsPresent()
    {
        Assert.Contains("CreatePaymentIntent", DashboardCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Backend_ConfirmPaymentHandler_IsPresent()
    {
        Assert.Contains("ConfirmPayment", DashboardCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Backend_StripePublishableKey_ExposedToView()
    {
        Assert.Contains("StripePublishableKey", DashboardCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Backend_MarkInvoicePaidAsync_CalledOnConfirm()
    {
        Assert.Contains("UpdateInvoiceAsync", DashboardCode, StringComparison.Ordinal);
    }

    // ── helpers ─────────────────────────────────────────────────────────────
    private static string ReadRepoFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "RodneyPortfolio.sln")))
                return File.ReadAllText(Path.Combine(dir.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar)));
            dir = dir.Parent;
        }
        throw new InvalidOperationException("Could not locate repository root.");
    }
}
