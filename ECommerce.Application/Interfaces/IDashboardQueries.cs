using ECommerce.Application.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Interfaces
{
    public interface IDashboardQueries
    {
        Task<DashboardSummaryDto> GetSummaryAsync(int lowStockThreshold, CancellationToken ct);
        Task<List<SalesPointDto>> GetSalesAsync(int days, CancellationToken ct);
        Task<List<TopProductDto>> GetTopProductsAsync(int days, int take, CancellationToken ct);
        Task<List<LowStockProductDto>> GetLowStockAsync(int threshold, int take, CancellationToken ct);

    }
}
