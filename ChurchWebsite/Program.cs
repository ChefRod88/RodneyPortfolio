//I dedicate this website and commit it to My Lord and Savior Jesus Christ to the glory of God the Father being led by the Holy Spirit. To advance the kingdom of God and the spread of the gospel.
// To make disciples of all nations. Baptize them in the name of the Father, the Son, and the Holy Spirit.
// Let your will be done ABBA. Amen. 
// Church Website - Application entry point

// USE CASE: Configure DI, services, routing; add new pages/services here
using ChurchWebsite.Hubs;
using ChurchWebsite.Models;
using ChurchWebsite.Services;

var builder = WebApplication.CreateBuilder(args);

// SERVICES
builder.Services.AddRazorPages();                                    // Enables Razor Pages (Index, About, etc.)
builder.Services.AddSignalR();                                       // Real-time WebSocket hub for location streaming
builder.Services.AddHttpContextAccessor();                           // Needed for _Layout to read current path
builder.Services.Configure<ChurchSettings>(builder.Configuration.GetSection(ChurchSettings.SectionName));  // Binds appsettings Church section
builder.Services.Configure<PexelsOptions>(builder.Configuration.GetSection(PexelsOptions.SectionName));
builder.Services.Configure<ChurchImagerySettings>(builder.Configuration.GetSection(ChurchImagerySettings.SectionName));
builder.Services.AddHttpClient<IPexelsPhotoClient, PexelsPhotoClient>(client =>
{
    client.BaseAddress = new Uri("https://api.pexels.com/");
});
builder.Services.AddSingleton<ChurchImageryRegistry>();
builder.Services.AddSingleton<IChurchImageryRegistry>(sp => sp.GetRequiredService<ChurchImageryRegistry>());
builder.Services.AddHostedService<ChurchImageryWarmupHostedService>();
builder.Services.AddScoped<ISermonService, SermonService>();         // In-memory sermons; inject in Index, Sermons pages
builder.Services.AddScoped<IEventService, EventService>();           // In-memory events; inject in Events pages
builder.Services.AddScoped<IGroupService, GroupService>();           // In-memory groups; inject in Groups pages
builder.Services.AddHttpClient<ILocationService, LocationService>(); // Server-side IP lookup for Location page

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
app.MapStaticAssets();                           // Serves wwwroot (css, js, images)
app.MapHub<LocationHub>("/hubs/location");       // SignalR WebSocket endpoint for real-time GPS streaming
app.MapRazorPages()                              // Maps /Index, /About, /Events/Index, etc.
   .WithStaticAssets();

app.Run();
