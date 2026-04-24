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
        private readonly ISlotRepository _slotRepository;
        private readonly ITankRepository _tankRepository;
        private readonly IClinicalThresholdsAccessor _clinicalThresholdsAccessor;

        public GetDashboardQueryHandler(
            IPatientRepository patientRepository,
            ICollectionSessionRepository sessionRepository,
            IBagRepository bagRepository,
            ISlotRepository slotRepository,
            ITankRepository tankRepository,
            IClinicalThresholdsAccessor clinicalThresholdsAccessor)
        {
            _patientRepository = patientRepository;
            _sessionRepository = sessionRepository;
            _bagRepository = bagRepository;
            _slotRepository = slotRepository;
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
            var slots = await _slotRepository.GetListAsync(
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
                TotalSlots = slots.Count,
                OccupiedSlots = slots.Items.Count(s => s.IsOccupied),
                EmptySlots = slots.Items.Count(s => !s.IsOccupied),
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
