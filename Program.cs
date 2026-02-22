using RodneyPortfolio.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IResumeContextLoader, ResumeContextLoader>();
builder.Services.AddScoped<IAIChatService, OpenAIChatService>();

var app = builder.Build();

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

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.UseStaticFiles();
app.MapControllers();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
