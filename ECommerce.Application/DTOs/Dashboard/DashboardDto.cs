namespace ECommerce.Application.DTOs.Dashboard;

public record DashboardSummaryDto(
    decimal TotalRevenue,
    decimal RevenueLast30Days,
    int TotalOrders,
    int PendingOrders,
    int NewCustomersLast30Days,
    int ActiveProducts,
    int LowStockProducts,
    Dictionary<string, int> OrdersByStatus);

public record SalesPointDto(DateOnly Date, decimal Revenue, int Orders);

public record TopProductDto(
    int ProductId, string ProductName, string Sku, int QuantitySold, decimal Revenue);

public record LowStockProductDto(int Id, string Name, string Sku, int StockQuantity);