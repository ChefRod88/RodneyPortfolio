using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;
using RodneyPortfolio.ViewModels;
using Stripe;
using InvoiceModel = RodneyPortfolio.Models.Invoice;

namespace RodneyPortfolio.Controllers;

[Route("Portal")]
public class PortalController : Controller
{
    private const string SessionEmailKey     = "otp_email";
    private const string SessionPurposeKey   = "otp_purpose";
    private const string DevBypassSessionKey = "rc_dev_bypass_email";

    private readonly IAccountService          _accounts;
    private readonly IOtpService              _otp;
    private readonly ISessionService          _sessions;
    private readonly IPortalEmailService      _email;
    private readonly IInvoiceService          _invoiceService;
    private readonly IWebHostEnvironment      _env;
    private readonly StripeOptions            _stripeOptions;
    private readonly ILogger<PortalController> _logger;

    public PortalController(
        IAccountService accounts,
        IOtpService otp,
        ISessionService sessions,
        IPortalEmailService email,
        IInvoiceService invoiceService,
        IWebHostEnvironment env,
        IOptions<StripeOptions> stripeOptions,
        ILogger<PortalController> logger)
    {
        _accounts       = accounts;
        _otp            = otp;
        _sessions       = sessions;
        _email          = email;
        _invoiceService = invoiceService;
        _env            = env;
        _stripeOptions  = stripeOptions.Value;
        _logger         = logger;
    }

    // ── Portal landing ────────────────────────────────────────────────────────
    [HttpGet("")]
    public IActionResult Index() => View();

    // ── Register ──────────────────────────────────────────────────────────────
    [HttpGet("Register")]
    public IActionResult Register() => View(new PortalRegisterViewModel());

    [HttpPost("Register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        [Bind(Prefix = "Input")] RegisterInput input, CancellationToken ct)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(new PortalRegisterViewModel { Input = input });

            if (await _accounts.EmailExistsAsync(input.Email, ct))
            {
                ModelState.AddModelError(string.Empty, "An account with that email already exists. Please sign in instead.");
                return View(new PortalRegisterViewModel { Input = input });
            }

            var account = new ClientAccount
            {
                FirstName      = input.FirstName,
                LastName       = input.LastName,
                Email          = input.Email,
                Phone          = input.Phone,
                CompanyName    = input.CompanyName,
                BillingAddress = input.BillingAddress,
                City           = input.City,
                State          = input.State,
                ZipCode        = input.ZipCode,
                TierInterest   = input.TierInterest,
                Status         = "Pending"
            };

            await _accounts.SaveAccountAsync(account, ct);

            var code = await _otp.GenerateOtpAsync(input.Email, "register", ct);
            await _email.SendOtpAsync(input.Email, input.FirstName, code, "register", ct);

            _logger.LogInformation("Registration OTP sent to {Email}", input.Email);

            return RedirectToAction("Verify", new { email = input.Email, purpose = "register" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", input.Email);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            return View(new PortalRegisterViewModel { Input = input });
        }
    }

    // ── Login ─────────────────────────────────────────────────────────────────
    [HttpGet("Login")]
    public IActionResult Login() => View(new PortalLoginViewModel());

    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        [Bind(Prefix = "Input")] LoginInput input, CancellationToken ct)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(new PortalLoginViewModel { Input = input });

            var account = await _accounts.GetByEmailAsync(input.Email, ct);

            if (account is null)
            {
                ModelState.AddModelError(string.Empty, "No account found with that email. Please register first.");
                return View(new PortalLoginViewModel { Input = input });
            }

            if (!account.IsVerified)
            {
                ModelState.AddModelError(string.Empty, "Your account is not yet verified. Please complete registration first.");
                return View(new PortalLoginViewModel { Input = input });
            }

            var code = await _otp.GenerateOtpAsync(input.Email, "login", ct);
            await _email.SendOtpAsync(input.Email, account.FirstName, code, "login", ct);

            return RedirectToAction("Verify", new { email = input.Email, purpose = "login" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", input.Email);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            return View(new PortalLoginViewModel { Input = input });
        }
    }

    // ── Verify ────────────────────────────────────────────────────────────────
    [HttpGet("Verify")]
    public IActionResult Verify(string email, string purpose = "login")
    {
        HttpContext.Session.SetString(SessionEmailKey,   email);
        HttpContext.Session.SetString(SessionPurposeKey, purpose);
        return View(new PortalVerifyViewModel { Email = email });
    }

    [HttpPost("Verify")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyCode(string code, CancellationToken ct)
    {
        var email   = HttpContext.Session.GetString(SessionEmailKey)   ?? string.Empty;
        var purpose = HttpContext.Session.GetString(SessionPurposeKey) ?? "login";

        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return View("Verify", new PortalVerifyViewModel { Email = email, ErrorMessage = "Session expired. Please start again." });

            var valid = await _otp.ValidateOtpAsync(email, code, purpose, ct);
            if (!valid)
                return View("Verify", new PortalVerifyViewModel { Email = email, ErrorMessage = "Invalid or expired code. Please try again or request a new one." });

            var account = await _accounts.GetByEmailAsync(email, ct);
            if (account is null)
                return View("Verify", new PortalVerifyViewModel { Email = email, ErrorMessage = "Account not found. Please register again." });

            if (purpose == "register")
            {
                account.VerifiedAt = DateTimeOffset.UtcNow;
                account.Status = "Active";
                await _accounts.SaveAccountAsync(account, ct);
                await _email.SendWelcomeAsync(account, ct);
            }

            account.LastLoginAt = DateTimeOffset.UtcNow;
            await _accounts.SaveAccountAsync(account, ct);

            var session = await _sessions.CreateSessionAsync(account.Id, account.Email, ct);
            Response.Cookies.Append("rc_portal_session", session.Id, new CookieOptions
            {
                HttpOnly = true,
                Secure   = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires  = session.ExpiresAt
            });

            return RedirectToAction("Dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OTP verification for {Email}", email);
            return View("Verify", new PortalVerifyViewModel { Email = email, ErrorMessage = "An unexpected error occurred. Please try again." });
        }
    }

    [HttpPost("Verify/Resend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendCode(CancellationToken ct)
    {
        var email   = HttpContext.Session.GetString(SessionEmailKey)   ?? string.Empty;
        var purpose = HttpContext.Session.GetString(SessionPurposeKey) ?? "login";

        try
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                var account = await _accounts.GetByEmailAsync(email, ct);
                if (account is not null)
                {
                    var code = await _otp.GenerateOtpAsync(email, purpose, ct);
                    await _email.SendOtpAsync(email, account.FirstName, code, purpose, ct);
                }
            }

            return View("Verify", new PortalVerifyViewModel { Email = email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending OTP for {Email}", email);
            return View("Verify", new PortalVerifyViewModel { Email = email, ErrorMessage = "Failed to resend code. Please try again." });
        }
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────
    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        try
        {
            var account = await ResolveCurrentAccountAsync(ct);
            if (account is null)
                return RedirectToAction("Login");

            return View(await BuildDashboardViewModelAsync(account, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            return RedirectToAction("Login");
        }
    }

    [HttpPost("Dashboard/Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        try
        {
            if (Request.Cookies.TryGetValue("rc_portal_session", out var sessionId) &&
                !string.IsNullOrWhiteSpace(sessionId))
            {
                await _sessions.InvalidateSessionAsync(sessionId, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating session during logout");
        }

        Response.Cookies.Delete("rc_portal_session");
        return RedirectToAction("Login");
    }

    [HttpPost("Dashboard/Support")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Support(
        [Bind(Prefix = "SupportMsg")] SupportMessageInput supportMsg, CancellationToken ct)
    {
        try
        {
            var account = await ResolveCurrentAccountAsync(ct);
            if (account is null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                var errVm = await BuildDashboardViewModelAsync(account, ct);
                errVm.SupportMsg = supportMsg;
                return View("Dashboard", errVm);
            }

            await _email.SendSupportMessageAsync(account, supportMsg, ct);

            var vm = await BuildDashboardViewModelAsync(account, ct);
            vm.StatusMessage = "Support message sent successfully.";
            return View("Dashboard", vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending support message");
            return new JsonResult(new { error = "Failed to send support message. Please try again." }) { StatusCode = 500 };
        }
    }

    // ── Stripe: Create Payment Intent ─────────────────────────────────────────
    [HttpPost("Dashboard/CreatePaymentIntent")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePaymentIntent(
        [FromBody] CreatePaymentIntentRequest req, CancellationToken ct)
    {
        try
        {
            var account = await ResolveCurrentAccountAsync(ct);
            if (account is null)
                return new JsonResult(new { error = "Unauthorized" }) { StatusCode = 401 };

            var allInvoices = await _invoiceService.GetAllInvoicesAsync(ct);
            var invoice = allInvoices.FirstOrDefault(i =>
                i.Id == req.InvoiceId &&
                string.Equals(i.ClientEmail, account.Email, StringComparison.OrdinalIgnoreCase));

            if (invoice is null)
                return new JsonResult(new { error = "Invoice not found" }) { StatusCode = 404 };

            if (invoice.Status == InvoiceStatus.Paid)
                return new JsonResult(new { error = "Invoice is already paid." }) { StatusCode = 400 };

            StripeConfiguration.ApiKey = _stripeOptions.SecretKey;

            var options = new PaymentIntentCreateOptions
            {
                Amount   = (long)Math.Round(invoice.Amount * 100, 0, MidpointRounding.AwayFromZero),
                Currency = "usd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
                Metadata = new Dictionary<string, string>
                {
                    ["invoiceId"]     = invoice.Id,
                    ["clientEmail"]   = account.Email,
                    ["invoiceNumber"] = invoice.InvoiceNumber ?? invoice.Id[..8].ToUpper()
                }
            };

            var service = new PaymentIntentService();
            var intent  = await service.CreateAsync(options, cancellationToken: ct);

            return new JsonResult(new { clientSecret = intent.ClientSecret });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment intent for invoice {InvoiceId}", req?.InvoiceId);
            return new JsonResult(new { error = "Failed to create payment intent. Please try again." }) { StatusCode = 500 };
        }
    }

    // ── Stripe: Confirm Payment ───────────────────────────────────────────────
    [HttpPost("Dashboard/ConfirmPayment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment(
        [FromBody] ConfirmPaymentRequest req, CancellationToken ct)
    {
        try
        {
            var account = await ResolveCurrentAccountAsync(ct);
            if (account is null)
                return new JsonResult(new { error = "Unauthorized" }) { StatusCode = 401 };

            var allInvoices = await _invoiceService.GetAllInvoicesAsync(ct);
            var invoice = allInvoices.FirstOrDefault(i =>
                i.Id == req.InvoiceId &&
                string.Equals(i.ClientEmail, account.Email, StringComparison.OrdinalIgnoreCase));

            if (invoice is null)
                return new JsonResult(new { error = "Invoice not found" }) { StatusCode = 404 };

            StripeConfiguration.ApiKey = _stripeOptions.SecretKey;
            var paymentIntent = await new PaymentIntentService().GetAsync(req.PaymentIntentId, cancellationToken: ct);

            if (paymentIntent is null || paymentIntent.Status != "succeeded")
                return new JsonResult(new { error = "Payment is not yet completed." }) { StatusCode = 400 };

            if (paymentIntent.Currency is not "usd")
                return new JsonResult(new { error = "Unexpected payment currency." }) { StatusCode = 400 };

            if (paymentIntent.AmountReceived < (long)Math.Round(invoice.Amount * 100, 0, MidpointRounding.AwayFromZero))
                return new JsonResult(new { error = "Payment amount does not cover this invoice." }) { StatusCode = 400 };

            if (paymentIntent.Metadata is null ||
                !paymentIntent.Metadata.TryGetValue("invoiceId", out var metaInvoiceId) ||
                !string.Equals(metaInvoiceId, invoice.Id, StringComparison.Ordinal))
                return new JsonResult(new { error = "Payment invoice verification failed." }) { StatusCode = 400 };

            // Delegate all "mark paid" field assignments to the service — single source of truth
            await _invoiceService.MarkInvoicePaidAsync(invoice.Id, req.PaymentIntentId, ct);

            _ = _email.SendReceiptAsync(account, invoice, ct)
                .ContinueWith(t => _logger.LogError(t.Exception, "Receipt email failed for invoice {InvoiceId}", invoice.Id),
                    TaskContinuationOptions.OnlyOnFaulted);

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm payment for {InvoiceId}", req.InvoiceId);
            return new JsonResult(new { error = "Payment confirmation failed." }) { StatusCode = 500 };
        }
    }

    // ── Cash App: Mark Pending ────────────────────────────────────────────────
    [HttpPost("Dashboard/CashAppPending")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CashAppPending(
        [FromBody] CashAppPendingRequest req, CancellationToken ct)
    {
        try
        {
            var account = await ResolveCurrentAccountAsync(ct);
            if (account is null)
                return new JsonResult(new { error = "Unauthorized" }) { StatusCode = 401 };

            var allInvoices = await _invoiceService.GetAllInvoicesAsync(ct);
            var invoice = allInvoices.FirstOrDefault(i =>
                i.Id == req.InvoiceId &&
                string.Equals(i.ClientEmail, account.Email, StringComparison.OrdinalIgnoreCase));

            if (invoice is null)
                return new JsonResult(new { error = "Invoice not found" }) { StatusCode = 404 };

            invoice.Status        = InvoiceStatus.PendingCashApp;
            invoice.PaymentMethod = "CashApp";

            await _invoiceService.UpdateInvoiceAsync(invoice, ct);

            _ = _email.SendCashAppPendingAsync(account, invoice, ct)
                .ContinueWith(t => _logger.LogError(t.Exception, "CashApp pending email failed for invoice {InvoiceId}", invoice.Id),
                    TaskContinuationOptions.OnlyOnFaulted);

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CashApp pending for invoice {InvoiceId}", req?.InvoiceId);
            return new JsonResult(new { error = "Failed to process Cash App payment request. Please try again." }) { StatusCode = 500 };
        }
    }

    // ── Private Helpers ───────────────────────────────────────────────────────
    private async Task<ClientAccount?> ResolveCurrentAccountAsync(CancellationToken ct)
    {
        try
        {
            if (_env.IsDevelopment())
            {
                var queryBypass = Request.Query["devBypassEmail"].ToString().Trim();
                if (!string.IsNullOrWhiteSpace(queryBypass))
                {
                    HttpContext.Session.SetString(DevBypassSessionKey, queryBypass);
                    return await EnsureDevelopmentBypassAccountAsync(queryBypass, ct);
                }

                var sessionBypass = HttpContext.Session.GetString(DevBypassSessionKey);
                if (!string.IsNullOrWhiteSpace(sessionBypass))
                    return await EnsureDevelopmentBypassAccountAsync(sessionBypass, ct);
            }

            if (!Request.Cookies.TryGetValue("rc_portal_session", out var sessionId) ||
                string.IsNullOrWhiteSpace(sessionId))
                return null;

            var session = await _sessions.GetSessionAsync(sessionId, ct);
            if (session is null) return null;

            return await _accounts.GetByIdAsync(session.ClientId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving current account");
            return null;
        }
    }

    private async Task<ClientAccount?> EnsureDevelopmentBypassAccountAsync(string email, CancellationToken ct)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail)) return null;

        var account = await _accounts.GetByEmailAsync(normalizedEmail, ct);
        if (account is null)
        {
            account = new ClientAccount
            {
                FirstName      = "Dev",
                LastName       = "Tester",
                Email          = normalizedEmail,
                Phone          = "555-0100",
                CompanyName    = "Local QA",
                BillingAddress = "123 Test Street",
                City           = "Testville",
                State          = "TX",
                ZipCode        = "75001",
                TierInterest   = "Starter",
                Status         = "Active"
            };
            await _accounts.SaveAccountAsync(account, ct);
        }

        var existingInvoices = await _invoiceService.GetAllInvoicesAsync(ct);
        var hasOpenInvoice = existingInvoices.Any(i =>
            string.Equals(i.ClientEmail, account.Email, StringComparison.OrdinalIgnoreCase) &&
            i.Status is InvoiceStatus.Unpaid or InvoiceStatus.Overdue or InvoiceStatus.PendingCashApp);

        if (!hasOpenInvoice)
        {
            await _invoiceService.SaveInvoiceAsync(new InvoiceModel
            {
                ClientId      = account.Id,
                ClientName    = account.FullName,
                ClientEmail   = account.Email,
                Description   = "Development Stripe flow validation",
                Amount        = 42.42m,
                IssuedAt      = DateTimeOffset.UtcNow,
                DueAt         = DateTimeOffset.UtcNow.AddDays(7),
                Status        = InvoiceStatus.Unpaid,
                InvoiceNumber = $"DEV-{DateTimeOffset.UtcNow:yyyyMMddHHmm}"
            }, ct);
        }

        return account;
    }

    private async Task<PortalDashboardViewModel> BuildDashboardViewModelAsync(ClientAccount account, CancellationToken ct)
    {
        var allInvoices = await _invoiceService.GetAllInvoicesAsync(ct);
        var accountInvoices = allInvoices
            .Where(i => string.Equals(i.ClientEmail, account.Email, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(i => i.IssuedAt)
            .ToList();

        return new PortalDashboardViewModel
        {
            Account      = account,
            OpenInvoices = accountInvoices
                .Where(i => i.Status is InvoiceStatus.Unpaid or InvoiceStatus.Overdue or InvoiceStatus.PendingCashApp)
                .ToList(),
            PaidInvoices = accountInvoices
                .Where(i => i.Status == InvoiceStatus.Paid)
                .ToList(),
            StripePublishableKey = _stripeOptions.PublishableKey
        };
    }
}
