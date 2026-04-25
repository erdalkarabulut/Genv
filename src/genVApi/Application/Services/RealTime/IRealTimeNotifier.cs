namespace Application.Services.RealTime;

public interface IRealTimeNotifier
{
    Task BagStoredAsync(Guid bagId, Guid bagCellId, CancellationToken cancellationToken = default);
    Task BagMovedAsync(Guid bagId, Guid fromBagCellId, Guid toBagCellId, CancellationToken cancellationToken = default);
    Task BagUsedAsync(Guid bagId, Guid? fromBagCellId, CancellationToken cancellationToken = default);
    Task DashboardUpdatedAsync(CancellationToken cancellationToken = default);
}
