using ECommerce.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.API.Hubs;

[Authorize(Roles = Roles.Admin)]
public class AdminHub : Hub<IAdminClient>
{
    public const string AdminsGroup = "admins";

    private readonly ILogger<AdminHub> _logger;

    public AdminHub(ILogger<AdminHub> logger) => _logger = logger;

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, AdminsGroup);

        _logger.LogInformation(
            "Admin connected to notifications hub. UserId: {UserId}",
            Context.User?.FindFirst("sub")?.Value);

        await base.OnConnectedAsync();
    }
}