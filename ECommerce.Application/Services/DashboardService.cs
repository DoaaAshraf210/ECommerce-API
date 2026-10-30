using ECommerce.Application.Common.Caching;
using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private const int DefaultLowStockThreshold = 5;
        private static readonly string[] Tags = { CacheTags.Dashboard };

        private readonly IDashboardQueries _queries;
        private readonly ICacheService _cache;

        public DashboardService(IDashboardQueries queries, ICacheService cache)
        {
            _queries = queries;
            _cache = cache;
        }

        public Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default)
            => _cache.GetOrCreateAsync(
                CacheKeys.DashboardSummary,
                token => _queries.GetSummaryAsync(DefaultLowStockThreshold, token),
                TimeSpan.FromSeconds(60), Tags, ct);

        public Task<List<SalesPointDto>> GetSalesAsync(int days, CancellationToken ct = default)
        {
            days = Math.Clamp(days, 1, 90);

            return _cache.GetOrCreateAsync(
                CacheKeys.DashboardSales(days),
                token => _queries.GetSalesAsync(days, token),
                TimeSpan.FromMinutes(5), Tags, ct);
        }

        public Task<List<TopProductDto>> GetTopProductsAsync(
       int days, int take, CancellationToken ct = default)
        {
            days = Math.Clamp(days, 1, 90);
            take = Math.Clamp(take, 1, 20);

            return _cache.GetOrCreateAsync(
                CacheKeys.DashboardTopProducts(days, take),
                token => _queries.GetTopProductsAsync(days, take, token),
                TimeSpan.FromMinutes(5), Tags, ct);
        }

        public Task<List<LowStockProductDto>> GetLowStockAsync(
            int threshold, int take, CancellationToken ct = default)
        {
            threshold = Math.Clamp(threshold, 0, 1000);
            take = Math.Clamp(take, 1, 50);

            return _cache.GetOrCreateAsync(
                CacheKeys.DashboardLowStock(threshold, take),
                token => _queries.GetLowStockAsync(threshold, take, token),
                TimeSpan.FromMinutes(2), Tags, ct);
        }
    }
}
