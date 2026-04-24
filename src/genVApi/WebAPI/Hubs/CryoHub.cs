using Microsoft.AspNetCore.SignalR;

namespace WebAPI.Hubs;

public class CryoHub : Hub
{
    public Task JoinTenant(string tenantId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"tenant-{tenantId}");

    public Task LeaveTenant(string tenantId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tenant-{tenantId}");
}
