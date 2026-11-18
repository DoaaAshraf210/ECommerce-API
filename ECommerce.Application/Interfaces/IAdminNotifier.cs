using ECommerce.Application.DTOs.Notifications;

namespace ECommerce.Application.Interfaces;

public interface IAdminNotifier
{
    Task OrderCreatedAsync(OrderCreatedNotification notification, CancellationToken ct = default);
    Task LowStockAsync(IReadOnlyList<LowStockNotification> items, CancellationToken ct = default);
}