using ECommerce.Application.DTOs.Notifications;

namespace ECommerce.API.Hubs;

public interface IAdminClient
{
    Task OrderCreated(OrderCreatedNotification notification);
    Task LowStock(LowStockNotification notification);
}