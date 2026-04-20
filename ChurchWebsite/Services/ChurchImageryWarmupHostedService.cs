using ChurchWebsite.Models;
using Microsoft.Extensions.Options;

namespace ChurchWebsite.Services;

/// <summary>Loads Pexels search results for all configured slots before the app accepts traffic.</summary>
public sealed class ChurchImageryWarmupHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ChurchImageryWarmupHostedService> _log;

    public ChurchImageryWarmupHostedService(IServiceProvider services, ILogger<ChurchImageryWarmupHostedService> log)
    {
        _services = services;
        _log = log;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var pexelsOptions = scope.ServiceProvider.GetRequiredService<IOptions<PexelsOptions>>().Value;
        if (string.IsNullOrWhiteSpace(pexelsOptions.ApiKey))
        {
            _log.LogWarning("Pexels ApiKey is not set; church imagery slots will be empty. Use user secrets or Pexels__ApiKey.");
            return;
        }

        var imagerySettings = scope.ServiceProvider.GetRequiredService<IOptions<ChurchImagerySettings>>().Value;
        var client = scope.ServiceProvider.GetRequiredService<IPexelsPhotoClient>();
        var registry = scope.ServiceProvider.GetRequiredService<ChurchImageryRegistry>();

        var queries = imagerySettings.Queries;
        if (queries.Count == 0)
            return;

        var parallel = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = 4
        };

        await Parallel.ForEachAsync(queries, parallel, async (kv, ct) =>
        {
            var photo = await client.SearchFirstAsync(kv.Value, ct).ConfigureAwait(false);
            if (photo is not null)
                registry.Set(kv.Key, photo);
            else
                _log.LogWarning("No Pexels image resolved for slot {Slot} (query: {Query})", kv.Key, kv.Value);
        }).ConfigureAwait(false);

        _log.LogInformation("Church Pexels imagery warmup finished ({Count} slots).", registry.LoadedCount);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
