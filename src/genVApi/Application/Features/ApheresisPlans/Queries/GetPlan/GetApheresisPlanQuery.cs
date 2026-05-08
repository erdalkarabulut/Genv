using Application.Services.ClinicalConfiguration;
using Application.Services.Repositories;
using Domain.Entities;
using Domain.ValueObjects;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;

namespace Application.Features.ApheresisPlans.Queries.GetPlan;

public class GetApheresisPlanQuery : IRequest<ApheresisPlanResponse>, ICachableRequest, ILoggableRequest
{
    public Guid PatientId { get; set; }

    public bool BypassCache { get; }
    public string CacheKey => $"ApheresisPlan(PatientId={PatientId})";
    public string? CacheGroupKey => "GetPatients";
    public TimeSpan? SlidingExpiration { get; }

    public class GetApheresisPlanQueryHandler : IRequestHandler<GetApheresisPlanQuery, ApheresisPlanResponse>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ICollectionSessionRepository _sessionRepository;
        private readonly IClinicalThresholdsAccessor _clinicalThresholdsAccessor;

        public GetApheresisPlanQueryHandler(
            IPatientRepository patientRepository,
            ICollectionSessionRepository sessionRepository,
            IClinicalThresholdsAccessor clinicalThresholdsAccessor)
        {
            _patientRepository = patientRepository;
            _sessionRepository = sessionRepository;
            _clinicalThresholdsAccessor = clinicalThresholdsAccessor;
        }

        public async Task<ApheresisPlanResponse> Handle(GetApheresisPlanQuery request, CancellationToken cancellationToken)
        {
            ClinicalThresholds thresholds = await _clinicalThresholdsAccessor.GetAsync(cancellationToken);
            Patient? patient = await _patientRepository.GetAsync(
                predicate: p => p.Id == request.PatientId,
                cancellationToken: cancellationToken
            );
            if (patient is null)
                throw new BusinessException("Patient not found.");

            List<CollectionSession> sessions = await _sessionRepository.Query()
                .AsNoTracking()
                .Where(s => s.PatientId == request.PatientId)
                .OrderBy(s => s.Day)
                .ToListAsync(cancellationToken);

            patient.Sessions = sessions;

            double cumulativeCd34 = 0;
            double cumulativeCd3 = 0;
            List<ApheresisDayDto> completed = new();
            foreach (var s in sessions)
            {
                cumulativeCd34 += s.Cd34PerKg;
                cumulativeCd3 += s.Cd3PerKg;
                completed.Add(new ApheresisDayDto
                {
                    Day = s.Day,
                    Date = s.Date,
                    Cd34PerKg = s.Cd34PerKg,
                    Cd3PerKg = s.Cd3PerKg,
                    CumulativeCd34 = cumulativeCd34,
                    CumulativeCd3 = cumulativeCd3,
                    SessionId = s.Id,
                    WbcPre = s.WbcPre,
                    Hgb = s.Hgb,
                    Hct = s.Hct,
                    Plt = s.Plt,
                    PreCd45Percent = s.PreCd45Percent,
                    PreCd34Percent = s.PreCd34Percent,
                    PreMhs = s.PreMhs,
                    WbcPost = s.WbcPost,
                    HgbPost = s.HgbPost,
                    HctPost = s.HctPost,
                    PltPost = s.PltPost,
                    VolumeMl = s.VolumeMl,
                    Wbc = s.WBC,
                    Cd34Percent = s.Cd34Percent,
                    Cd45Percent = s.Cd45Percent,
                    Cd3Percent = s.Cd3Percent,
                    LymphocytePercent = s.LymphocytePercent,
                    Mhs = s.Mhs,
                    AbsoluteCellCount = s.AbsoluteCellCount
                });
            }

            int maxDays = patient.GetMaxCollectionDays(thresholds);
            int lastDay = sessions.Count == 0 ? 0 : sessions.Max(s => s.Day);
            double target = patient.GetTargetCd34(thresholds);
            double ideal = patient.GetIdealCd34(thresholds);
            bool isSufficient = cumulativeCd34 >= target;
            bool isOptimal = cumulativeCd34 >= ideal;
            bool maxReached = lastDay >= maxDays;
            bool shouldContinue = !isSufficient && !maxReached;

            DateTime? nextPlannedDate = null;
            int? nextDay = null;
            if (shouldContinue)
            {
                nextDay = lastDay + 1;
                if (sessions.Count == 0)
                    nextPlannedDate = DateTime.UtcNow.Date.AddDays(1);
                else
                {
                    var last = sessions.OrderByDescending(s => s.Day).First();
                    nextPlannedDate = last.Date.Date.AddDays(1);
                }
            }

            string recommendation = BuildRecommendation(
                isSufficient: isSufficient,
                isOptimal: isOptimal,
                maxReached: maxReached,
                lastDay: lastDay,
                maxDays: maxDays,
                transplantType: patient.TransplantType,
                cumulativeCd34: cumulativeCd34,
                target: target
            );

            List<ApheresisDayDto> plan = BuildForecast(
                completed: completed,
                nextDay: nextDay,
                maxDays: maxDays,
                nextPlannedDate: nextPlannedDate
            );

            return new ApheresisPlanResponse
            {
                PatientId = patient.Id,
                PatientName = patient.FullName,
                WeightKg = patient.WeightKg,
                TransplantType = patient.TransplantType.ToString(),
                IsAutologous = patient.IsAutologous(),
                MaxCollectionDays = maxDays,
                TargetCd34PerKg = target,
                IdealCd34PerKg = ideal,
                CompletedDays = lastDay,
                RemainingDays = Math.Max(0, maxDays - lastDay),
                CumulativeCd34PerKg = Math.Round(cumulativeCd34, 2),
                CumulativeCd3PerKg = Math.Round(cumulativeCd3, 2),
                IsSufficient = isSufficient,
                IsOptimal = isOptimal,
                MaxDaysReached = maxReached,
                ShouldContinue = shouldContinue,
                NextDay = nextDay,
                NextPlannedDate = nextPlannedDate,
                Status = patient.GetStatus(thresholds),
                Recommendation = recommendation,
                CompletedSessions = completed,
                ForecastPlan = plan
            };
        }

        private static string BuildRecommendation(
            bool isSufficient,
            bool isOptimal,
            bool maxReached,
            int lastDay,
            int maxDays,
            TransplantType transplantType,
            double cumulativeCd34,
            double target)
        {
            if (lastDay == 0)
            {
                var type = transplantType == TransplantType.Autologous ? "otolog (hasta kendi)" : "allogeneik (donör)";
                return $"Aferez henüz başlamadı. {type} protokolünde maksimum {maxDays} gün numune alınabilir. Gün 1 planlanmalı.";
            }

            if (isOptimal)
                return $"Kümülatif CD34 ({cumulativeCd34:F2}) ideal aralıkta. Aferez tamamlanabilir.";

            if (isSufficient)
                return $"Kümülatif CD34 ({cumulativeCd34:F2}) minimum hedefi ({target:F2}) karşılıyor. Klinik değerlendirme gerekir.";

            if (maxReached)
                return $"Maksimum {maxDays} güne ulaşıldı ancak kümülatif CD34 ({cumulativeCd34:F2}) hedefin ({target:F2}) altında. Klinik değerlendirme gerekir.";

            return $"Gün {lastDay} tamamlandı, kümülatif CD34 ({cumulativeCd34:F2}) hedefin ({target:F2}) altında. Gün {lastDay + 1} ile devam edilmelidir.";
        }

        private static List<ApheresisDayDto> BuildForecast(
            List<ApheresisDayDto> completed,
            int? nextDay,
            int maxDays,
            DateTime? nextPlannedDate)
        {
            List<ApheresisDayDto> plan = new(completed);
            if (!nextDay.HasValue) return plan;

            DateTime? date = nextPlannedDate;
            for (int d = nextDay.Value; d <= maxDays; d++)
            {
                plan.Add(new ApheresisDayDto
                {
                    Day = d,
                    Date = date ?? DateTime.UtcNow.Date,
                    Cd34PerKg = 0,
                    Cd3PerKg = 0,
                    CumulativeCd34 = plan.LastOrDefault()?.CumulativeCd34 ?? 0,
                    CumulativeCd3 = plan.LastOrDefault()?.CumulativeCd3 ?? 0,
                    SessionId = null,
                    IsPlanned = true
                });
                date = date?.AddDays(1);
            }
            return plan;
        }
    }
}

public class ApheresisPlanResponse
{
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = default!;
    public double WeightKg { get; set; }
    public string TransplantType { get; set; } = default!;
    public bool IsAutologous { get; set; }

    public int MaxCollectionDays { get; set; }
    public double TargetCd34PerKg { get; set; }
    public double IdealCd34PerKg { get; set; }

    public int CompletedDays { get; set; }
    public int RemainingDays { get; set; }

    public double CumulativeCd34PerKg { get; set; }
    public double CumulativeCd3PerKg { get; set; }

    public bool IsSufficient { get; set; }
    public bool IsOptimal { get; set; }
    public bool MaxDaysReached { get; set; }
    public bool ShouldContinue { get; set; }

    public int? NextDay { get; set; }
    public DateTime? NextPlannedDate { get; set; }

    public string Status { get; set; } = default!;
    public string Recommendation { get; set; } = default!;

    public List<ApheresisDayDto> CompletedSessions { get; set; } = new();
    public List<ApheresisDayDto> ForecastPlan { get; set; } = new();
}

public class ApheresisDayDto
{
    public int Day { get; set; }
    public DateTime Date { get; set; }
    public double Cd34PerKg { get; set; }
    public double Cd3PerKg { get; set; }
    public double CumulativeCd34 { get; set; }
    public double CumulativeCd3 { get; set; }
    public Guid? SessionId { get; set; }
    public bool IsPlanned { get; set; }

    // PK (pre-procedure) — aferez öncesi kan paneli + flow cytometry
    public double? WbcPre { get; set; }
    public double? Hgb { get; set; }
    public double? Hct { get; set; }
    public double? Plt { get; set; }
    public double? PreCd45Percent { get; set; }
    public double? PreCd34Percent { get; set; }
    public double? PreMhs { get; set; }

    // İşlem sonrası hemogram (opsiyonel)
    public double? WbcPost { get; set; }
    public double? HgbPost { get; set; }
    public double? HctPost { get; set; }
    public double? PltPost { get; set; }

    // ÜRÜN (post-procedure) — aferez ürünü ölçümleri
    public double? VolumeMl { get; set; }
    public double? Wbc { get; set; }
    public double? Cd34Percent { get; set; }
    public double? Cd45Percent { get; set; }
    public double? Cd3Percent { get; set; }
    public double? LymphocytePercent { get; set; }
    public double? Mhs { get; set; }
    public double? AbsoluteCellCount { get; set; }
}
