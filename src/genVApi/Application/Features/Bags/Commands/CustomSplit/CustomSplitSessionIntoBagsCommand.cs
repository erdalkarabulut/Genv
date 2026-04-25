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

namespace Application.Features.Bags.Commands.CustomSplit;

/// <summary>
/// Aferez ürününü kullanıcı tarafından belirlenen torba konfigürasyonuna göre böler.
/// Her torba kendi hacim, WBC ve yüzde değerlerine sahip olabilir; slot verilirse
/// doğrudan Frozen (dondurulmuş) statüsüyle slota yerleştirilir.
/// </summary>
public class CustomSplitSessionIntoBagsCommand : IRequest<CustomSplitSessionIntoBagsResponse>,
    ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid SessionId { get; set; }

    /// <summary>Kullanıcının tanımladığı torba konfigürasyonları (en az 1).</summary>
    public List<CustomBagSpec> Bags { get; set; } = new();

    /// <summary>false ise, torbaların toplam hacmi session hacmini aşabilir.</summary>
    public bool ValidateTotalVolume { get; set; } = true;

    public string[] Roles => [Admin, Write];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBags", "GetBagCells", "GetCollectionSessions", "GetPatients", "Dashboard"];

    public class CustomSplitSessionIntoBagsCommandHandler : IRequestHandler<CustomSplitSessionIntoBagsCommand, CustomSplitSessionIntoBagsResponse>
    {
        private readonly ICollectionSessionRepository _sessionRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IBagRepository _bagRepository;
        private readonly IBagCellRepository _bagCellRepository;
        private readonly IBagMovementRepository _movementRepository;
        private readonly IClinicalThresholdsAccessor _clinicalThresholdsAccessor;
        private readonly IRealTimeNotifier _notifier;

        public CustomSplitSessionIntoBagsCommandHandler(
            ICollectionSessionRepository sessionRepository,
            IPatientRepository patientRepository,
            IBagRepository bagRepository,
            IBagCellRepository bagCellRepository,
            IBagMovementRepository movementRepository,
            IClinicalThresholdsAccessor clinicalThresholdsAccessor,
            IRealTimeNotifier notifier)
        {
            _sessionRepository = sessionRepository;
            _patientRepository = patientRepository;
            _bagRepository = bagRepository;
            _bagCellRepository = bagCellRepository;
            _movementRepository = movementRepository;
            _clinicalThresholdsAccessor = clinicalThresholdsAccessor;
            _notifier = notifier;
        }

        public async Task<CustomSplitSessionIntoBagsResponse> Handle(CustomSplitSessionIntoBagsCommand request, CancellationToken cancellationToken)
        {
            if (request.Bags is null || request.Bags.Count == 0)
                throw new BusinessException("En az bir torba konfigürasyonu gereklidir.");

            CollectionSession? session = await _sessionRepository.GetAsync(
                predicate: s => s.Id == request.SessionId,
                cancellationToken: cancellationToken
            );
            if (session is null)
                throw new BusinessException("Aferez seansı bulunamadı.");

            Patient? patient = await _patientRepository.GetAsync(
                predicate: p => p.Id == session.PatientId,
                cancellationToken: cancellationToken
            );
            if (patient is null || patient.WeightKg <= 0)
                throw new BusinessException("Hasta bulunamadı veya kilo bilgisi eksik.");

            ClinicalThresholds thresholds = await _clinicalThresholdsAccessor.GetAsync(cancellationToken);

            double totalRequested = request.Bags.Sum(b => b.VolumeMl);
            if (request.ValidateTotalVolume && session.VolumeMl > 0 && totalRequested > session.VolumeMl + 0.001)
                throw new BusinessException(
                    $"Torbaların toplam hacmi ({totalRequested:F2} ml) aferez ürün hacmini ({session.VolumeMl:F2} ml) aşıyor.");

            // Tek seferde aynı batch içinde oluşturulan torbaları gruplayabilmek için batchId üret.
            Guid splitBatchId = Guid.NewGuid();

            // Mevcut en yüksek bag numarasını öğrenip üstüne ekle.
            int maxBagNumber = await _bagRepository.Query()
                .AsNoTracking()
                .Where(b => b.SessionId == session.Id)
                .Select(b => (int?)b.BagNumber)
                .MaxAsync(cancellationToken) ?? 0;

            // Aynı slota çift yerleştirmeyi önlemek için kullanılan slot id'leri.
            HashSet<Guid> usedBagCellIds = new();

            List<Bag> created = new();
            List<CustomSplitBagResultDto> results = new();

            for (int i = 0; i < request.Bags.Count; i++)
            {
                CustomBagSpec spec = request.Bags[i];
                if (spec.VolumeMl <= 0)
                    throw new BusinessException($"#{i + 1}. torba için hacim > 0 olmalı.");

                // Varsayılan değerler session'dan alınır; kullanıcı override edebilir.
                double wbc = spec.Wbc ?? session.WBC;
                double cd45 = spec.Cd45Percent ?? session.Cd45Percent;
                double cd34 = spec.Cd34Percent ?? session.Cd34Percent;
                double? cd3 = spec.Cd3Percent ?? (session.Cd3Percent > 0 ? session.Cd3Percent : (double?)null);

                // Bu torbanın kendi hacim/WBC/yüzdeleriyle CD34/kg hesabı.
                double cd34PerKg = thresholds.ComputeSessionCd34PerKg(
                    spec.VolumeMl,
                    wbc,
                    cd45,
                    cd34,
                    patient.WeightKg);
                double cd3PerKg = cd3.HasValue
                    ? thresholds.ComputeSessionCd3PerKg(spec.VolumeMl, wbc, cd3.Value, patient.WeightKg)
                    : 0;

                Bag bag = new()
                {
                    SessionId = session.Id,
                    BagNumber = maxBagNumber + i + 1,
                    VolumeMl = Math.Round(spec.VolumeMl, 2),
                    SourceVolumeMl = session.VolumeMl,
                    Wbc = wbc,
                    Cd45Percent = cd45,
                    Cd34Percent = cd34,
                    Cd3Percent = cd3,
                    CompositionNote = spec.CompositionNote,
                    Cd34PerKg = Math.Round(cd34PerKg, 4),
                    Cd3PerKg = Math.Round(cd3PerKg, 4),
                    Purpose = spec.Purpose,
                    SplitBatchId = splitBatchId,
                    Status = BagStatus.Reserved
                };

                // İsteğe bağlı slot/dondurma.
                Guid? bagCellId = null;
                if (spec.FreezeIntoBagCellId.HasValue)
                {
                    if (!usedBagCellIds.Add(spec.FreezeIntoBagCellId.Value))
                        throw new BusinessException("Aynı torba hücresi birden fazla torba için seçilemez.");

                    BagCell? cell = await _bagCellRepository.GetAsync(
                        predicate: c => c.Id == spec.FreezeIntoBagCellId.Value,
                        cancellationToken: cancellationToken
                    );
                    if (cell is null)
                        throw new BusinessException("Seçilen torba hücresi bulunamadı.");
                    if (cell.IsOccupied)
                        throw new BusinessException($"Torba hücresi ({cell.Position}) dolu; boş bir hücre seçin.");

                    cell.IsOccupied = true;
                    cell.Version += 1;
                    await _bagCellRepository.UpdateAsync(cell);

                    bag.BagCellId = cell.Id;
                    bag.Status = BagStatus.Frozen;
                    bagCellId = cell.Id;
                }

                await _bagRepository.AddAsync(bag);
                created.Add(bag);

                if (bagCellId.HasValue)
                {
                    await _movementRepository.AddAsync(new BagMovement
                    {
                        BagId = bag.Id,
                        FromBagCellId = null,
                        ToBagCellId = bagCellId.Value,
                        Action = "CustomSplit-Freeze"
                    });

                    await _notifier.BagStoredAsync(bag.Id, bagCellId.Value, cancellationToken);
                }

                results.Add(new CustomSplitBagResultDto
                {
                    BagId = bag.Id,
                    BagNumber = bag.BagNumber,
                    VolumeMl = bag.VolumeMl,
                    Cd34PerKg = bag.Cd34PerKg,
                    Cd3PerKg = bag.Cd3PerKg,
                    Purpose = bag.Purpose.ToString(),
                    Status = bag.Status.ToString(),
                    BagCellId = bag.BagCellId
                });
            }

            await _notifier.DashboardUpdatedAsync(cancellationToken);

            return new CustomSplitSessionIntoBagsResponse
            {
                SessionId = session.Id,
                PatientId = patient.Id,
                SplitBatchId = splitBatchId,
                TotalVolumeMl = Math.Round(totalRequested, 2),
                BagCount = created.Count,
                Bags = results
            };
        }
    }
}

public class CustomBagSpec
{
    public double VolumeMl { get; set; }
    public double? Wbc { get; set; }
    public double? Cd45Percent { get; set; }
    public double? Cd34Percent { get; set; }
    public double? Cd3Percent { get; set; }
    public BagPurpose Purpose { get; set; } = BagPurpose.Cryo;

    /// <summary>El yazısı kayıtlardaki bölünme notu (ör. "32+32=64 ml").</summary>
    public string? CompositionNote { get; set; }

    /// <summary>Verilirse torba oluşturulduktan sonra bu slota Frozen olarak yerleştirilir.</summary>
    public Guid? FreezeIntoBagCellId { get; set; }
}

public class CustomSplitSessionIntoBagsResponse
{
    public Guid SessionId { get; set; }
    public Guid PatientId { get; set; }
    public Guid SplitBatchId { get; set; }
    public double TotalVolumeMl { get; set; }
    public int BagCount { get; set; }
    public List<CustomSplitBagResultDto> Bags { get; set; } = new();
}

public class CustomSplitBagResultDto
{
    public Guid BagId { get; set; }
    public int BagNumber { get; set; }
    public double VolumeMl { get; set; }
    public double Cd34PerKg { get; set; }
    public double Cd3PerKg { get; set; }
    public string Purpose { get; set; } = default!;
    public string Status { get; set; } = default!;
    public Guid? BagCellId { get; set; }
}
