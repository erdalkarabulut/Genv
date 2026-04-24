using Application.Features.Bags.Constants;
using Application.Services.ClinicalConfiguration;
using Application.Services.RealTime;
using Application.Services.Repositories;
using Domain.Entities;
using Domain.ValueObjects;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using static Application.Features.Bags.Constants.BagsOperationClaims;

namespace Application.Features.Bags.Commands.Split;

/// <summary>
/// Aferez ürünü kümülatif olarak yeterli hale geldiğinde session'ın ürününü
/// N torbaya böler (varsayılan 4) ve 1 tanesini Cryo amacıyla işaretler.
/// İsteğe bağlı <see cref="CryoSlotId"/> verilirse Cryo torbası otomatik olarak
/// o slota yerleştirilir (Store işlemi + BagMovement logu + SignalR).
/// </summary>
public class SplitSessionIntoBagsCommand : IRequest<SplitSessionIntoBagsResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid SessionId { get; set; }
    public int BagCount { get; set; } = 4;

    /// <summary>Belirtilirse cryo torbası doğrudan bu slota store edilir.</summary>
    public Guid? CryoSlotId { get; set; }

    /// <summary>
    /// true ve <see cref="CryoSlotId"/> verilmediyse, sistem ilk boş slotu otomatik olarak seçer (Auto-Place).
    /// </summary>
    public bool AutoPlaceCryo { get; set; } = true;

    /// <summary>false ise patient'ın kümülatif CD34'ü yeterli değilken de bölme yapılabilir.</summary>
    public bool RequireCumulativeSufficient { get; set; } = true;

    public string[] Roles => [Admin, Write];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBags", "GetSlots", "GetCollectionSessions", "GetPatients", "Dashboard"];

    public class SplitSessionIntoBagsCommandHandler : IRequestHandler<SplitSessionIntoBagsCommand, SplitSessionIntoBagsResponse>
    {
        private readonly ICollectionSessionRepository _sessionRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IBagRepository _bagRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly IBagMovementRepository _movementRepository;
        private readonly IClinicalThresholdsAccessor _clinicalThresholdsAccessor;
        private readonly IRealTimeNotifier _notifier;

        public SplitSessionIntoBagsCommandHandler(
            ICollectionSessionRepository sessionRepository,
            IPatientRepository patientRepository,
            IBagRepository bagRepository,
            ISlotRepository slotRepository,
            IBagMovementRepository movementRepository,
            IClinicalThresholdsAccessor clinicalThresholdsAccessor,
            IRealTimeNotifier notifier)
        {
            _sessionRepository = sessionRepository;
            _patientRepository = patientRepository;
            _bagRepository = bagRepository;
            _slotRepository = slotRepository;
            _movementRepository = movementRepository;
            _clinicalThresholdsAccessor = clinicalThresholdsAccessor;
            _notifier = notifier;
        }

        public async Task<SplitSessionIntoBagsResponse> Handle(SplitSessionIntoBagsCommand request, CancellationToken cancellationToken)
        {
            if (request.BagCount < 2)
                throw new BusinessException("En az 2 torbaya bölünebilir. Öneri: 4.");

            CollectionSession? session = await _sessionRepository.GetAsync(
                predicate: s => s.Id == request.SessionId,
                cancellationToken: cancellationToken
            );
            if (session is null)
                throw new BusinessException("CollectionSession bulunamadı.");

            bool alreadySplit = await _bagRepository.Query()
                .AsNoTracking()
                .AnyAsync(b => b.SessionId == session.Id && b.SplitBatchId != null, cancellationToken);
            if (alreadySplit)
                throw new BusinessException("Bu aferez seansı zaten torbalara bölünmüş.");

            Patient? patient = await _patientRepository.GetAsync(
                predicate: p => p.Id == session.PatientId,
                cancellationToken: cancellationToken
            );
            if (patient is null)
                throw new BusinessException("Hasta bulunamadı.");

            List<CollectionSession> allSessions = await _sessionRepository.Query()
                .AsNoTracking()
                .Where(s => s.PatientId == patient.Id)
                .ToListAsync(cancellationToken);
            patient.Sessions = allSessions;

            ClinicalThresholds thresholds = await _clinicalThresholdsAccessor.GetAsync(cancellationToken);

            if (request.RequireCumulativeSufficient && !patient.IsSufficient(thresholds))
                throw new BusinessException(
                    $"Kümülatif CD34 ({patient.GetTotalCd34():F2}) hedefin ({patient.GetTargetCd34(thresholds):F2}) altında. " +
                    $"Torbalara bölme yapılamaz. (RequireCumulativeSufficient=false göndererek zorlayabilirsiniz.)");

            if (session.VolumeMl <= 0)
                throw new BusinessException("Session hacmi 0 olduğundan bölme yapılamaz.");

            Guid splitBatchId = Guid.NewGuid();
            double perBagVolume = Math.Round(session.VolumeMl / request.BagCount, 2);
            double perBagCd34 = Math.Round(session.Cd34PerKg / request.BagCount, 4);
            double perBagCd3 = Math.Round(session.Cd3PerKg / request.BagCount, 4);

            List<Bag> created = new();
            for (int i = 1; i <= request.BagCount; i++)
            {
                BagPurpose purpose = ResolvePurpose(i);
                var bag = new Bag
                {
                    SessionId = session.Id,
                    BagNumber = i,
                    VolumeMl = perBagVolume,
                    SourceVolumeMl = session.VolumeMl,
                    Wbc = session.WBC,
                    Cd34Percent = session.Cd34Percent,
                    Cd45Percent = session.Cd45Percent,
                    Cd3Percent = session.Cd3Percent,
                    Cd34PerKg = perBagCd34,
                    Cd3PerKg = perBagCd3,
                    Status = purpose == BagPurpose.Cryo ? BagStatus.Frozen : BagStatus.Reserved,
                    Purpose = purpose,
                    SplitBatchId = splitBatchId
                };
                await _bagRepository.AddAsync(bag);
                created.Add(bag);
            }

            Bag cryoBag = created.First(b => b.Purpose == BagPurpose.Cryo);

            Slot? slot = null;
            if (request.CryoSlotId.HasValue)
            {
                slot = await _slotRepository.GetAsync(
                    predicate: s => s.Id == request.CryoSlotId.Value,
                    cancellationToken: cancellationToken
                );
                if (slot is null)
                    throw new BusinessException("Cryo slotu bulunamadı.");
                if (slot.IsOccupied)
                    throw new BusinessException("Cryo slotu dolu. Boş bir slot seçin.");
            }
            else if (request.AutoPlaceCryo)
            {
                slot = await _slotRepository.Query()
                    .Where(s => !s.IsOccupied)
                    .OrderBy(s => s.Position)
                    .FirstOrDefaultAsync(cancellationToken);
                if (slot is null)
                    throw new BusinessException("Boş slot bulunamadı (Auto-Place başarısız).");
            }

            if (slot is not null)
            {

                slot.IsOccupied = true;
                slot.Version += 1;
                cryoBag.SlotId = slot.Id;
                cryoBag.Status = BagStatus.Stored;

                await _slotRepository.UpdateAsync(slot);
                await _bagRepository.UpdateAsync(cryoBag);
                await _movementRepository.AddAsync(new BagMovement
                {
                    BagId = cryoBag.Id,
                    FromSlotId = null,
                    ToSlotId = slot.Id,
                    Action = "Split-Store (Cryo)"
                });

                await _notifier.BagStoredAsync(cryoBag.Id, slot.Id, cancellationToken);
            }

            await _notifier.DashboardUpdatedAsync(cancellationToken);

            return new SplitSessionIntoBagsResponse
            {
                SessionId = session.Id,
                PatientId = patient.Id,
                SplitBatchId = splitBatchId,
                BagCount = request.BagCount,
                PerBagVolumeMl = perBagVolume,
                PerBagCd34PerKg = perBagCd34,
                PerBagCd3PerKg = perBagCd3,
                CryoBagId = cryoBag.Id,
                CryoSlotId = cryoBag.SlotId,
                Bags = created.Select(b => new SplitBagDto
                {
                    BagId = b.Id,
                    BagNumber = b.BagNumber,
                    VolumeMl = b.VolumeMl,
                    Cd34PerKg = b.Cd34PerKg,
                    Cd3PerKg = b.Cd3PerKg,
                    Purpose = b.Purpose.ToString(),
                    Status = b.Status.ToString(),
                    SlotId = b.SlotId
                }).ToList()
            };
        }

        /// <summary>
        /// 4'lü bölüşüm varsayılanı: 1=Cryo, 2=Infusion, 3=Backup, 4=QualityControl.
        /// Farklı sayıda bölme yapılırsa: 1 Cryo, kalanlar Infusion → Backup → QC döngüsü.
        /// </summary>
        private static BagPurpose ResolvePurpose(int index) => index switch
        {
            1 => BagPurpose.Cryo,
            2 => BagPurpose.Infusion,
            3 => BagPurpose.Backup,
            4 => BagPurpose.QualityControl,
            _ => (BagPurpose)(((index - 1) % 4))
        };
    }
}

public class SplitSessionIntoBagsResponse
{
    public Guid SessionId { get; set; }
    public Guid PatientId { get; set; }
    public Guid SplitBatchId { get; set; }
    public int BagCount { get; set; }
    public double PerBagVolumeMl { get; set; }
    public double PerBagCd34PerKg { get; set; }
    public double PerBagCd3PerKg { get; set; }
    public Guid CryoBagId { get; set; }
    public Guid? CryoSlotId { get; set; }
    public List<SplitBagDto> Bags { get; set; } = new();
}

public class SplitBagDto
{
    public Guid BagId { get; set; }
    public int BagNumber { get; set; }
    public double VolumeMl { get; set; }
    public double Cd34PerKg { get; set; }
    public double Cd3PerKg { get; set; }
    public string Purpose { get; set; } = default!;
    public string Status { get; set; } = default!;
    public Guid? SlotId { get; set; }
}
