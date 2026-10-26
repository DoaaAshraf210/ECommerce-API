using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Caching
{
    public class HybridCacheService : ICacheService
    {
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan MaxLocalExpiration = TimeSpan.FromSeconds(30);

        private readonly HybridCache _cache;
        private readonly ILogger<HybridCacheService> _logger;

        public HybridCacheService(HybridCache cache, ILogger<HybridCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T> GetOrCreateAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? expiration = null,
            IEnumerable<string>? tags = null,
            CancellationToken ct = default)
        {
            var ttl = expiration ?? DefaultExpiration;
            var options = new HybridCacheEntryOptions
            {
                Expiration = ttl,
                LocalCacheExpiration = ttl < MaxLocalExpiration ? ttl : MaxLocalExpiration
            };

            Exception? factoryError = null;

            try
            {
                return await _cache.GetOrCreateAsync(
                    key,
                    async token =>
                    {
                        try { return await factory(token); }
                        catch (Exception ex) { factoryError = ex; throw; }
                    },
                    options,
                    tags,
                    ct);
            }
            catch (Exception ex) when (factoryError is null && ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex,
                    "Cache unavailable for key {CacheKey}. Falling back to the database.", key);

                return await factory(ct);
            }
        }

        public async Task RemoveByTagAsync(string tag, CancellationToken ct = default)
        {
            try
            {
                await _cache.RemoveByTagAsync(tag, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex,
                    "Failed to invalidate cache tag {Tag}. Data may be stale until it expires.", tag);
            }
        }
    }
}
