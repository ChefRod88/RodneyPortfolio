// Church Website - Application entry point
// USE CASE: Configure DI, services, routing; add new pages/services here
using ChurchWebsite.Models;
using ChurchWebsite.Services;

var builder = WebApplication.CreateBuilder(args);

// SERVICES
builder.Services.AddRazorPages();                                    // Enables Razor Pages (Index, About, etc.)
builder.Services.AddHttpContextAccessor();                           // Needed for _Layout to read current path
builder.Services.Configure<ChurchSettings>(builder.Configuration.GetSection(ChurchSettings.SectionName));  // Binds appsettings Church section
builder.Services.AddScoped<SermonService>();                         // In-memory sermons; inject in Index, Sermons pages
builder.Services.AddScoped<EventService>();                          // In-memory events; inject in Events pages
builder.Services.AddScoped<GroupService>();                         // In-memory groups; inject in Groups pages

var app = builder.Build();

// HTTP PIPELINE - Order matters
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");  // Non-dev: show Error page instead of stack trace
    app.UseHsts();                      // Add HSTS header for HTTPS
}

app.UseHttpsRedirection();   // Redirect HTTP to HTTPS
app.UseRouting();            // Enable endpoint routing
app.UseAuthorization();      // Auth middleware (no auth configured yet)

// ROUTING
app.MapStaticAssets();       // Serves wwwroot (css, js, images)
app.MapRazorPages()          // Maps /Index, /About, /Events/Index, etc.
   .WithStaticAssets();

app.Run();
