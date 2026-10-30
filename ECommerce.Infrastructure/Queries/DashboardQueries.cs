using ECommerce.Application.Common.Constants;
using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace ECommerce.Infrastructure.Queries
{
    public class DashboardQueries : IDashboardQueries
    {
        // الطلبات اللي بتتحسب إيراد
        private static readonly OrderStatus[] RevenueStatuses =
            { OrderStatus.Paid, OrderStatus.Shipped, OrderStatus.Delivered };

        private readonly ApplicationDbContext _db;

        public DashboardQueries(ApplicationDbContext db) => _db = db;

        public async Task<DashboardSummaryDto> GetSummaryAsync(int lowStockThreshold, CancellationToken ct)
        {
            var since = DateTime.UtcNow.AddDays(-30);
            var orders = _db.Orders.AsNoTracking();

            var counts = (await orders
                    .GroupBy(o => o.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync(ct))
                .ToDictionary(x => x.Status, x => x.Count);

            var totalRevenue = await orders
                .Where(o => RevenueStatuses.Contains(o.Status))
                .SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0m;

            var revenueLast30 = await orders
                .Where(o => o.CreatedAt >= since && RevenueStatuses.Contains(o.Status))
                .SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0m;

            var newCustomers = await (
                    from u in _db.Users
                    join ur in _db.UserRoles on u.Id equals ur.UserId
                    join r in _db.Roles on ur.RoleId equals r.Id
                    where r.Name == Roles.Customer && u.CreatedAt >= since
                    select u.Id)
                .CountAsync(ct);

            var activeProducts = await _db.Products.CountAsync(p => p.IsActive, ct);
            var lowStock = await _db.Products
                .CountAsync(p => p.IsActive && p.StockQuantity <= lowStockThreshold, ct);

            var byStatus = Enum.GetValues<OrderStatus>()
                .ToDictionary(s => s.ToString(), s => counts.GetValueOrDefault(s));

            return new DashboardSummaryDto(
                totalRevenue,
                revenueLast30,
                counts.Values.Sum(),
                counts.GetValueOrDefault(OrderStatus.Pending),
                newCustomers,
                activeProducts,
                lowStock,
                byStatus);
        }

        public async Task<List<SalesPointDto>> GetSalesAsync(int days, CancellationToken ct)
        {
            var start = DateTime.UtcNow.Date.AddDays(-(days - 1));

            var rows = await _db.Orders.AsNoTracking()
                .Where(o => o.CreatedAt >= start && RevenueStatuses.Contains(o.Status))
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Day = g.Key, Revenue = g.Sum(o => o.TotalAmount), Orders = g.Count() })
                .ToListAsync(ct);

            var byDay = rows.ToDictionary(r => DateOnly.FromDateTime(r.Day));

            return Enumerable.Range(0, days)
                .Select(i => DateOnly.FromDateTime(start.AddDays(i)))
                .Select(d => byDay.TryGetValue(d, out var r)
                    ? new SalesPointDto(d, r.Revenue, r.Orders)
                    : new SalesPointDto(d, 0m, 0))
                .ToList();
        }

        public async Task<List<TopProductDto>> GetTopProductsAsync(int days, int take, CancellationToken ct)
        {
            var since = DateTime.UtcNow.AddDays(-days);

            var rows = await _db.Orders.AsNoTracking()
                .Where(o => o.CreatedAt >= since && RevenueStatuses.Contains(o.Status))
                .SelectMany(o => o.Items)
                .GroupBy(i => i.ProductId)              
                .Select(g => new
                {
                    ProductId = g.Key,
                    Name = g.Max(i => i.ProductName),
                    Sku = g.Max(i => i.Sku),       
                    Qty = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.UnitPrice * i.Quantity)
                })
                .OrderByDescending(x => x.Qty).ThenBy(x => x.ProductId)
                .Take(take)
                .ToListAsync(ct);
            var ids = rows.Select(r => r.ProductId).ToList();

            var current = await _db.Products.IgnoreQueryFilters().AsNoTracking()
                .Where(p => ids.Contains(p.Id))
                .Select(p => new { p.Id, p.Name, p.Sku })
                .ToDictionaryAsync(p => p.Id, ct);

            return rows
                .Select(x => current.TryGetValue(x.ProductId, out var p)
                    ? new TopProductDto(x.ProductId, p.Name, p.Sku, x.Qty, x.Revenue)
                    : new TopProductDto(x.ProductId, x.Name, x.Sku, x.Qty, x.Revenue))
                .ToList();

        }

        public async Task<List<LowStockProductDto>> GetLowStockAsync(int threshold, int take, CancellationToken ct)
        {
            var rows = await _db.Products.AsNoTracking()
        .Where(p => p.IsActive && p.StockQuantity <= threshold) 
        .OrderBy(p => p.StockQuantity).ThenBy(p => p.Id)       
        .Take(take)                                               
        .Select(p => new { p.Id, p.Name, p.Sku, p.StockQuantity })
        .ToListAsync(ct);                                         

            return rows
                .Select(p => new LowStockProductDto(p.Id, p.Name, p.Sku, p.StockQuantity))
                .ToList();

        }
    }
}