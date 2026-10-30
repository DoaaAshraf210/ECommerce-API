using ECommerce.Application.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default);
        Task<List<SalesPointDto>> GetSalesAsync(int days, CancellationToken ct = default);
        Task<List<TopProductDto>> GetTopProductsAsync(int days, int take, CancellationToken ct = default);
        Task<List<LowStockProductDto>> GetLowStockAsync(int threshold, int take, CancellationToken ct = default);

    }
}
