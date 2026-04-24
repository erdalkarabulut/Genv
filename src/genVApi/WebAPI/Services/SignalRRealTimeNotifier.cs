using Application.Services.RealTime;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Hubs;

namespace WebAPI.Services;

public class SignalRRealTimeNotifier : IRealTimeNotifier
{
    private const string DefaultTenant = "tenant-default";
    private readonly IHubContext<CryoHub> _hub;

    public SignalRRealTimeNotifier(IHubContext<CryoHub> hub)
    {
        _hub = hub;
    }

    public Task BagStoredAsync(Guid bagId, Guid slotId, CancellationToken cancellationToken = default) =>
        _hub.Clients.Group(DefaultTenant).SendAsync("BagStored", new { bagId, slotId }, cancellationToken);

    public Task BagMovedAsync(Guid bagId, Guid fromSlotId, Guid toSlotId, CancellationToken cancellationToken = default) =>
        _hub.Clients.Group(DefaultTenant).SendAsync("BagMoved", new { bagId, fromSlotId, toSlotId }, cancellationToken);

    public Task BagUsedAsync(Guid bagId, Guid? fromSlotId, CancellationToken cancellationToken = default) =>
        _hub.Clients.Group(DefaultTenant).SendAsync("BagUsed", new { bagId, fromSlotId }, cancellationToken);

    public Task DashboardUpdatedAsync(CancellationToken cancellationToken = default) =>
        _hub.Clients.Group(DefaultTenant).SendAsync("DashboardUpdated", cancellationToken);
}
