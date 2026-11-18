namespace ECommerce.Application.DTOs.Notifications;

public record OrderCreatedNotification(
    int OrderId, string OrderNumber, decimal TotalAmount, int ItemsCount, DateTime CreatedAtUtc);

public record LowStockNotification(
    int ProductId, string ProductName, string Sku, int StockQuantity);