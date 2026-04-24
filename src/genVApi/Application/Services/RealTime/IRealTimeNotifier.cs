namespace Application.Services.RealTime;

public interface IRealTimeNotifier
{
    Task BagStoredAsync(Guid bagId, Guid slotId, CancellationToken cancellationToken = default);
    Task BagMovedAsync(Guid bagId, Guid fromSlotId, Guid toSlotId, CancellationToken cancellationToken = default);
    Task BagUsedAsync(Guid bagId, Guid? fromSlotId, CancellationToken cancellationToken = default);
    Task DashboardUpdatedAsync(CancellationToken cancellationToken = default);
}
