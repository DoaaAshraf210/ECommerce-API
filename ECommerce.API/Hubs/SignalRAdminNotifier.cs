using ECommerce.Application.DTOs.Notifications;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.API.Hubs;

public class SignalRAdminNotifier : IAdminNotifier
{
    private readonly IHubContext<AdminHub, IAdminClient> _hub;
    private readonly ILogger<SignalRAdminNotifier> _logger;

    public SignalRAdminNotifier(
        IHubContext<AdminHub, IAdminClient> hub, ILogger<SignalRAdminNotifier> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task OrderCreatedAsync(OrderCreatedNotification notification, CancellationToken ct = default)
    {
        try
        {
            await _hub.Clients.Group(AdminHub.AdminsGroup).OrderCreated(notification);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to push OrderCreated notification. OrderId: {OrderId}",
                notification.OrderId);
        }
    }

    public async Task LowStockAsync(IReadOnlyList<LowStockNotification> items, CancellationToken ct = default)
    {
        foreach (var item in items)
        {
            try
            {
                await _hub.Clients.Group(AdminHub.AdminsGroup).LowStock(item);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to push LowStock notification. ProductId: {ProductId}",
                    item.ProductId);
            }
        }
    }
}