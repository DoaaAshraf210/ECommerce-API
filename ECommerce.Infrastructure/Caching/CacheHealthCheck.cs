using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ECommerce.Infrastructure.Caching;

public class CacheHealthCheck : IHealthCheck
{
    private readonly IDistributedCache? _cache;

    public CacheHealthCheck(IServiceProvider services)
        => _cache = services.GetService<IDistributedCache>();

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken ct = default)
    {
        if (_cache is null)
            return HealthCheckResult.Healthy("Redis is not configured; using memory cache only.");

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(1));

            await _cache.GetAsync("health-probe", cts.Token);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("Cache backend is unavailable.", ex);
        }
    }
}