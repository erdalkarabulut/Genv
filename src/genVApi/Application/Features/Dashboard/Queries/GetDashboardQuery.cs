using Application.Services.ClinicalConfiguration;
using Application.Services.Repositories;
using Domain.Enums;
using MediatR;

namespace Application.Features.Dashboard.Queries;

public class GetDashboardQuery : IRequest<DashboardResponse>
{
    public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardResponse>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ICollectionSessionRepository _sessionRepository;
        private readonly IBagRepository _bagRepository;
        private readonly IBagCellRepository _bagCellRepository;
        private readonly ITankRepository _tankRepository;
        private readonly IClinicalThresholdsAccessor _clinicalThresholdsAccessor;

        public GetDashboardQueryHandler(
            IPatientRepository patientRepository,
            ICollectionSessionRepository sessionRepository,
            IBagRepository bagRepository,
            IBagCellRepository bagCellRepository,
            ITankRepository tankRepository,
            IClinicalThresholdsAccessor clinicalThresholdsAccessor)
        {
            _patientRepository = patientRepository;
            _sessionRepository = sessionRepository;
            _bagRepository = bagRepository;
            _bagCellRepository = bagCellRepository;
            _tankRepository = tankRepository;
            _clinicalThresholdsAccessor = clinicalThresholdsAccessor;
        }

        public async Task<DashboardResponse> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
        {
            var thresholds = await _clinicalThresholdsAccessor.GetAsync(cancellationToken);
            var patients = await _patientRepository.GetListAsync(
                size: int.MaxValue,
                enableTracking: false,
                cancellationToken: cancellationToken
            );
            var sessions = await _sessionRepository.GetListAsync(
                size: int.MaxValue,
                enableTracking: false,
                cancellationToken: cancellationToken
            );
            var bags = await _bagRepository.GetListAsync(
                size: int.MaxValue,
                enableTracking: false,
                cancellationToken: cancellationToken
            );
            var bagCells = await _bagCellRepository.GetListAsync(
                size: int.MaxValue,
                enableTracking: false,
                cancellationToken: cancellationToken
            );

            double totalCd34 = sessions.Items.Sum(s => s.Cd34PerKg);
            double totalCd3 = sessions.Items.Sum(s => s.Cd3PerKg);

            string risk = thresholds.ComputeAggregateRisk(totalCd34, totalCd3);

            return new DashboardResponse
            {
                TotalPatients = patients.Count,
                TotalSessions = sessions.Count,
                TotalBags = bags.Count,
                StoredBags = bags.Items.Count(b => b.Status == BagStatus.Stored),
                UsedBags = bags.Items.Count(b => b.Status == BagStatus.Used),
                FrozenBags = bags.Items.Count(b => b.Status == BagStatus.Frozen),
                ReservedBags = bags.Items.Count(b => b.Status == BagStatus.Reserved),
                DiscardedBags = bags.Items.Count(b => b.Status == BagStatus.Discarded),
                TotalSlots = bagCells.Count,
                OccupiedSlots = bagCells.Items.Count(c => c.IsOccupied),
                EmptySlots = bagCells.Items.Count(c => !c.IsOccupied),
                TotalCd34PerKg = totalCd34,
                TotalCd3PerKg = totalCd3,
                RiskStatus = risk
            };
        }
    }
}

public class DashboardResponse
{
    public int TotalPatients { get; set; }
    public int TotalSessions { get; set; }
    public int TotalBags { get; set; }
    public int StoredBags { get; set; }
    public int UsedBags { get; set; }
    public int FrozenBags { get; set; }
    public int ReservedBags { get; set; }
    public int DiscardedBags { get; set; }
    public int TotalSlots { get; set; }
    public int OccupiedSlots { get; set; }
    public int EmptySlots { get; set; }
    public double TotalCd34PerKg { get; set; }
    public double TotalCd3PerKg { get; set; }
    public string RiskStatus { get; set; } = "Normal";
}
