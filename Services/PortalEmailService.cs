using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public interface IPortalEmailService
{
    Task SendOtpAsync(string toEmail, string toName, string code, string purpose, CancellationToken ct = default);
    Task SendWelcomeAsync(ClientAccount account, CancellationToken ct = default);
    Task SendSupportMessageAsync(ClientAccount account, SupportMessageInput msg, CancellationToken ct = default);
    Task SendReceiptAsync(ClientAccount account, Invoice invoice, CancellationToken ct = default);
    Task SendCashAppPendingAsync(ClientAccount account, Invoice invoice, CancellationToken ct = default);
}

public class PortalEmailService : IPortalEmailService
{
    private readonly QuoteEmailOptions _options;
    private readonly ILogger<PortalEmailService> _logger;

    public PortalEmailService(IOptions<QuoteEmailOptions> options, ILogger<PortalEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendOtpAsync(string toEmail, string toName, string code, string purpose, CancellationToken ct = default)
    {
        var action = purpose == "register" ? "complete your registration" : "sign in to your account";
        var body = $"""
            <div style="background:#020c14;color:#e8f4f8;font-family:sans-serif;padding:2rem;max-width:480px;margin:0 auto;border:1px solid rgba(0,212,255,0.2)">
              <div style="font-size:0.6rem;letter-spacing:0.25em;color:#00d4ff;text-transform:uppercase;margin-bottom:1rem">RC Dev // Client Portal</div>
              <h2 style="color:#fff;font-size:1.3rem;margin:0 0 1rem">Your Verification Code</h2>
              <p style="color:rgba(142,207,223,0.7);line-height:1.7">Hi {toName}, use the code below to {action}. It expires in <strong style="color:#fff">10 minutes</strong>.</p>
              <div style="background:rgba(0,212,255,0.06);border:1px solid rgba(0,212,255,0.3);padding:2rem;text-align:center;margin:1.5rem 0">
                <div style="font-size:2.8rem;font-weight:900;letter-spacing:0.4em;color:#00d4ff;text-shadow:0 0 20px rgba(0,212,255,0.5)">{code}</div>
              </div>
              <p style="color:rgba(142,207,223,0.4);font-size:0.8rem">If you did not request this code, you can safely ignore this email.</p>
              <hr style="border-color:rgba(0,212,255,0.1);margin:1.5rem 0"/>
              <p style="color:rgba(142,207,223,0.3);font-size:0.7rem">RC Dev · rodneyachery.com</p>
            </div>
            """;
        await SendAsync(toEmail, $"[RC Dev] Your {(purpose == "register" ? "Registration" : "Sign-In")} Code: {code}", body);
    }

    public async Task SendWelcomeAsync(ClientAccount account, CancellationToken ct = default)
    {
        var body = $"""
            <div style="background:#020c14;color:#e8f4f8;font-family:sans-serif;padding:2rem;max-width:480px;margin:0 auto;border:1px solid rgba(0,212,255,0.2)">
              <div style="font-size:0.6rem;letter-spacing:0.25em;color:#00d4ff;text-transform:uppercase;margin-bottom:1rem">RC Dev // Client Portal</div>
              <h2 style="color:#fff;font-size:1.3rem">Welcome to RC Dev, {account.FirstName}!</h2>
              <p style="color:rgba(142,207,223,0.7);line-height:1.7">Your account has been created and verified. You can now sign in to your client portal to view invoices and make payments.</p>
              <p style="color:rgba(142,207,223,0.7)"><strong style="color:#fff">Service Interest:</strong> {account.TierInterest} Plan</p>
              <p style="color:rgba(142,207,223,0.5);font-size:0.85rem">I'll be in touch shortly to discuss your project and get everything set up. If you have any questions in the meantime, just reply to this email.</p>
              <a href="https://rodneyachery.com/Portal/Login" style="display:inline-block;background:#00d4ff;color:#000;padding:12px 24px;text-decoration:none;font-weight:bold;margin-top:1rem">Sign In to Portal →</a>
              <hr style="border-color:rgba(0,212,255,0.1);margin:1.5rem 0"/>
              <p style="color:rgba(142,207,223,0.3);font-size:0.7rem">RC Dev · rodneyachery.com</p>
            </div>
            """;
        await SendAsync(account.Email, "Welcome to RC Dev — Your Account Is Ready", body);
        // Notify yourself
        await SendAsync(_options.ToEmail, $"[RC Dev] New client registered: {account.FullName} ({account.TierInterest})", body);
    }

    public async Task SendSupportMessageAsync(ClientAccount account, SupportMessageInput msg, CancellationToken ct = default)
    {
        var body = $"""
            <div style="font-family:sans-serif;padding:1rem">
              <h3>Support Request from {account.FullName}</h3>
              <p><strong>Email:</strong> {account.Email}</p>
              <p><strong>Tier:</strong> {account.TierInterest}</p>
              <p><strong>Subject:</strong> {msg.Subject}</p>
              <hr/>
              <p>{msg.Message.Replace("\n", "<br/>")}</p>
            </div>
            """;
        await SendAsync(_options.ToEmail, $"[RC Dev Support] {msg.Subject} — {account.FullName}", body);
    }

    public async Task SendReceiptAsync(ClientAccount account, Invoice invoice, CancellationToken ct = default)
    {
        var body = $"""
            <div style="background:#020c14;color:#e8f4f8;font-family:sans-serif;padding:2rem;max-width:520px;margin:0 auto;border:1px solid rgba(0,212,255,0.2)">
              <div style="font-size:0.6rem;letter-spacing:0.25em;color:#00d4ff;text-transform:uppercase;margin-bottom:1rem">RC Dev // Payment Receipt</div>
              <h2 style="color:#fff;font-size:1.3rem;margin:0 0 1rem">Payment Received</h2>
              <p style="color:rgba(142,207,223,0.75);line-height:1.7">Hi {account.FirstName}, your payment has been received successfully.</p>
              <div style="background:rgba(0,212,255,0.06);border:1px solid rgba(0,212,255,0.3);padding:1rem 1.2rem;margin:1rem 0">
                <p style="margin:0.2rem 0"><strong>Invoice:</strong> #{invoice.Id[..8].ToUpperInvariant()}</p>
                <p style="margin:0.2rem 0"><strong>Description:</strong> {invoice.Description}</p>
                <p style="margin:0.2rem 0"><strong>Amount:</strong> ${invoice.Amount:F2}</p>
                <p style="margin:0.2rem 0"><strong>Paid At:</strong> {(invoice.PaidAt ?? DateTimeOffset.UtcNow):MMM dd, yyyy h:mm tt} UTC</p>
              </div>
              <p style="color:rgba(142,207,223,0.45);font-size:0.85rem">Thank you for your business.</p>
              <hr style="border-color:rgba(0,212,255,0.1);margin:1.5rem 0"/>
              <p style="color:rgba(142,207,223,0.3);font-size:0.7rem">RC Dev · rodneyachery.com</p>
            </div>
            """;

        await SendAsync(account.Email, $"Receipt — Invoice #{invoice.Id[..8].ToUpperInvariant()}", body);
    }

    public async Task SendCashAppPendingAsync(ClientAccount account, Invoice invoice, CancellationToken ct = default)
    {
        var body = $"""
            <div style="background:#020c14;color:#e8f4f8;font-family:sans-serif;padding:2rem;max-width:520px;margin:0 auto;border:1px solid rgba(0,212,255,0.2)">
              <div style="font-size:0.6rem;letter-spacing:0.25em;color:#00d4ff;text-transform:uppercase;margin-bottom:1rem">RC Dev // Payment Pending</div>
              <h2 style="color:#fff;font-size:1.3rem;margin:0 0 1rem">Cash App Payment Pending</h2>
              <p style="color:rgba(142,207,223,0.75);line-height:1.7">Hi {account.FirstName}, we are waiting for your Cash App payment confirmation.</p>
              <div style="background:rgba(0,212,255,0.06);border:1px solid rgba(0,212,255,0.3);padding:1rem 1.2rem;margin:1rem 0">
                <p style="margin:0.2rem 0"><strong>Invoice:</strong> #{invoice.Id[..8].ToUpperInvariant()}</p>
                <p style="margin:0.2rem 0"><strong>Description:</strong> {invoice.Description}</p>
                <p style="margin:0.2rem 0"><strong>Amount Due:</strong> ${invoice.Amount:F2}</p>
              </div>
              <p style="color:rgba(142,207,223,0.45);font-size:0.85rem">Once the payment is confirmed, you will receive a receipt email automatically.</p>
              <hr style="border-color:rgba(0,212,255,0.1);margin:1.5rem 0"/>
              <p style="color:rgba(142,207,223,0.3);font-size:0.7rem">RC Dev · rodneyachery.com</p>
            </div>
            """;

        await SendAsync(account.Email, $"Payment Pending — Invoice #{invoice.Id[..8].ToUpperInvariant()}", body);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var from = string.IsNullOrWhiteSpace(_options.FromEmail) ? _options.SmtpUsername : _options.FromEmail;
        using var msg = new MailMessage { From = new MailAddress(from, "RC Dev"), Subject = subject, Body = htmlBody, IsBodyHtml = true };
        msg.To.Add(toEmail);
        using var smtp = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword)
        };
        await smtp.SendMailAsync(msg);
        _logger.LogInformation("Portal email sent to {Email}: {Subject}", toEmail, subject);
    }
}