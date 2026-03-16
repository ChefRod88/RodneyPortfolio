//I dedicate this website and commit it to My Lord and Savior Jesus Christ to the glory of God the Father being led by the Holy Spirit. To advance the kingdom of God and the spread of the gospel.
// To make disciples of all nations. Baptize them in the name of the Father, the Son, and the Holy Spirit.
// Let your will be done ABBA. Amen. 


using RodneyPortfolio.Services;
using RodneyPortfolio.Models;
using RodneyPortfolio.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Load User Secrets in Development (API keys never committed to git)
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IResumeContextLoader, ResumeContextLoader>();
builder.Services.AddScoped<IOpenAIClient, OpenAIClient>();
builder.Services.AddScoped<IAnthropicClient, AnthropicClient>();

// Concrete registrations so DualAIChatService / DualJobMatchService can inject them directly
builder.Services.AddScoped<OpenAIChatService>();
builder.Services.AddScoped<AnthropicChatService>();
builder.Services.AddScoped<JobMatchService>();
builder.Services.AddScoped<AnthropicJobMatchService>();

// Public interfaces → Dual orchestrators (fire both AIs in parallel per request)
builder.Services.AddScoped<IAIChatService, DualAIChatService>();
builder.Services.AddScoped<IJobMatchService, DualJobMatchService>();
builder.Services.AddScoped<IInputValidator, InputValidator>();
builder.Services.AddScoped<IContentFilter, ContentFilter>();
builder.Services.Configure<QuoteEmailOptions>(builder.Configuration.GetSection(QuoteEmailOptions.SectionName));
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
// Quote submission — three single-responsibility services wired together
builder.Services.AddScoped<IQuoteLogService, QuoteLogService>();
builder.Services.AddScoped<IQuoteEmailService, QuoteEmailService>();
builder.Services.AddScoped<IQuoteSubmissionService, QuoteSubmissionService>();

builder.Services.AddScoped<IInvoiceService, SqlInvoiceService>();
builder.Services.AddScoped<IPaymentEmailService, PaymentEmailService>();

// Portal services — one concrete class, three focused interfaces (ISP)
// Registered once so DI returns the same scoped instance for all three interfaces.
builder.Services.AddScoped<SqlClientPortalService>();
builder.Services.AddScoped<IAccountService>(sp => sp.GetRequiredService<SqlClientPortalService>());
builder.Services.AddScoped<IOtpService>(sp => sp.GetRequiredService<SqlClientPortalService>());
builder.Services.AddScoped<ISessionService>(sp => sp.GetRequiredService<SqlClientPortalService>());
builder.Services.AddScoped<IPortalEmailService, PortalEmailService>();

// Rate limiting — protect quote form and chat API from abuse
builder.Services.AddRateLimiter(options =>
{
    // Quote form: max 5 submissions per IP per 10 minutes
    options.AddPolicy("QuotePolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Chat API: max 30 requests per IP per minute
    options.AddPolicy("ChatPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Session (for ASP.NET Core session middleware)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var sqlConnectionString =
    builder.Configuration.GetConnectionString("AzureSQL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

var hasSqlConnection = !string.IsNullOrWhiteSpace(sqlConnectionString);

if (hasSqlConnection)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(sqlConnectionString, sql =>
        {
            sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(15), errorNumbersToAdd: null);
        }));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("FallbackDb"));
}

var app = builder.Build();

if (hasSqlConnection)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupMigrations");
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed during startup. App will continue — DB-dependent features may fail at runtime.");
    }
}
else
{
    var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    startupLogger.LogWarning("No SQL connection string found. Running without database — portal/payment features will be unavailable.");
}

// STEP 6: Canonical domain + HTTPS redirect middleware
// - Forces https://
// - Forces www.
// Put this early in the pipeline so redirects happen before serving pages.
// Skip in Development or when using localhost/127.0.0.1 so you can view locally.
app.Use(async (context, next) =>
{
    var host = context.Request.Host.Host;
    var isLocal = host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
        || host.StartsWith("[::1]", StringComparison.OrdinalIgnoreCase);
    if (app.Environment.IsDevelopment() || isLocal)
    {
        await next();
        return;
    }

    var request = context.Request;

    // 1) Enforce HTTPS (handles cases where HTTPS Only isn't applied yet)
    var isHttps = request.IsHttps;

    // 2) Enforce "www" host (host already declared above)
    var isWww = host.StartsWith("www.", StringComparison.OrdinalIgnoreCase);

    // If host is your apex domain, redirect to www.
    // If not https, redirect to https.
    // NOTE: update "rodneyachery.com" + "www.rodneyachery.com" exactly to your domain.
    if (host.Equals("rodneyachery.com", StringComparison.OrdinalIgnoreCase) || !isWww || !isHttps)
    {
        var newHost = "www.rodneyachery.com";

        // Preserve path + querystring
        var newUrl =
            $"https://{newHost}{request.PathBase}{request.Path}{request.QueryString}";

        context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
        context.Response.Headers.Location = newUrl;
        return;
    }

    await next();
});


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Security headers on every response
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    await next();
});

app.UseRateLimiter();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller}/{action}/{id?}");
app.MapControllers();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
