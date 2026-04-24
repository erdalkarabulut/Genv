using Application.Features.ClinicalConfiguration;
using Application.Services.Repositories;
using MediatR;
using ClinicalEntity = Domain.Entities.ClinicalSettings;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Security.Constants;

namespace Application.Features.ClinicalConfiguration.Commands.UpdateClinicalSettings;

public class UpdateClinicalSettingsCommand : IRequest<ClinicalSettingsDto>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public double SessionCd34Cd3Divisor { get; set; }
    public double DliCd3CalculationDivisor { get; set; }
    public double TargetCd34AutologousPerKg { get; set; }
    public double TargetCd34AllogeneicPerKg { get; set; }
    public double IdealCd34AutologousPerKg { get; set; }
    public double IdealCd34AllogeneicPerKg { get; set; }
    public int MaxApheresisDaysAutologous { get; set; }
    public int MaxApheresisDaysAllogeneic { get; set; }
    public double DliHighDoseCd3PerKgThreshold { get; set; }
    public double ProductMinimumCd34PerKg { get; set; }
    public double DashboardCd34LowThreshold { get; set; }
    public double DashboardCd34ElevatedFloor { get; set; }
    public double DashboardCd3HighRiskThreshold { get; set; }
    public double DashboardCd3LowImmuneThreshold { get; set; }
    public double DashboardCd3OptimalMin { get; set; }
    public double DashboardCd3OptimalMax { get; set; }

    public string[] Roles => [GeneralOperationClaims.Admin];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey =>
    [
        "GetPatients",
        "Dashboard",
        "GetCollectionSessions",
        "GetDliProducts"
    ];

    public class Handler : IRequestHandler<UpdateClinicalSettingsCommand, ClinicalSettingsDto>
    {
        private readonly IClinicalSettingsRepository _repository;

        public Handler(IClinicalSettingsRepository repository) => _repository = repository;

        public async Task<ClinicalSettingsDto> Handle(UpdateClinicalSettingsCommand request, CancellationToken cancellationToken)
        {
            if (request.SessionCd34Cd3Divisor <= 0 || request.DliCd3CalculationDivisor <= 0)
                throw new BusinessException("Bölen değerleri 0'dan büyük olmalıdır.");

            if (request.MaxApheresisDaysAutologous < 1 || request.MaxApheresisDaysAllogeneic < 1)
                throw new BusinessException("Maksimum aferez gün sayısı en az 1 olmalıdır.");

            ClinicalEntity? row = await _repository.GetAsync(
                predicate: x => x.Id == ClinicalEntity.SingletonId,
                cancellationToken: cancellationToken);

            DateTime utc = DateTime.UtcNow;
            if (row is null)
            {
                row = new ClinicalEntity { Id = ClinicalEntity.SingletonId, CreatedDate = utc };
                Apply(request, row);
                await _repository.AddAsync(row);
            }
            else
            {
                Apply(request, row);
                row.UpdatedDate = utc;
                await _repository.UpdateAsync(row);
            }

            return new ClinicalSettingsDto
            {
                Id = row.Id,
                SessionCd34Cd3Divisor = row.SessionCd34Cd3Divisor,
                DliCd3CalculationDivisor = row.DliCd3CalculationDivisor,
                TargetCd34AutologousPerKg = row.TargetCd34AutologousPerKg,
                TargetCd34AllogeneicPerKg = row.TargetCd34AllogeneicPerKg,
                IdealCd34AutologousPerKg = row.IdealCd34AutologousPerKg,
                IdealCd34AllogeneicPerKg = row.IdealCd34AllogeneicPerKg,
                MaxApheresisDaysAutologous = row.MaxApheresisDaysAutologous,
                MaxApheresisDaysAllogeneic = row.MaxApheresisDaysAllogeneic,
                DliHighDoseCd3PerKgThreshold = row.DliHighDoseCd3PerKgThreshold,
                ProductMinimumCd34PerKg = row.ProductMinimumCd34PerKg,
                DashboardCd34LowThreshold = row.DashboardCd34LowThreshold,
                DashboardCd34ElevatedFloor = row.DashboardCd34ElevatedFloor,
                DashboardCd3HighRiskThreshold = row.DashboardCd3HighRiskThreshold,
                DashboardCd3LowImmuneThreshold = row.DashboardCd3LowImmuneThreshold,
                DashboardCd3OptimalMin = row.DashboardCd3OptimalMin,
                DashboardCd3OptimalMax = row.DashboardCd3OptimalMax
            };
        }

        private static void Apply(UpdateClinicalSettingsCommand request, ClinicalEntity row)
        {
            row.SessionCd34Cd3Divisor = request.SessionCd34Cd3Divisor;
            row.DliCd3CalculationDivisor = request.DliCd3CalculationDivisor;
            row.TargetCd34AutologousPerKg = request.TargetCd34AutologousPerKg;
            row.TargetCd34AllogeneicPerKg = request.TargetCd34AllogeneicPerKg;
            row.IdealCd34AutologousPerKg = request.IdealCd34AutologousPerKg;
            row.IdealCd34AllogeneicPerKg = request.IdealCd34AllogeneicPerKg;
            row.MaxApheresisDaysAutologous = request.MaxApheresisDaysAutologous;
            row.MaxApheresisDaysAllogeneic = request.MaxApheresisDaysAllogeneic;
            row.DliHighDoseCd3PerKgThreshold = request.DliHighDoseCd3PerKgThreshold;
            row.ProductMinimumCd34PerKg = request.ProductMinimumCd34PerKg;
            row.DashboardCd34LowThreshold = request.DashboardCd34LowThreshold;
            row.DashboardCd34ElevatedFloor = request.DashboardCd34ElevatedFloor;
            row.DashboardCd3HighRiskThreshold = request.DashboardCd3HighRiskThreshold;
            row.DashboardCd3LowImmuneThreshold = request.DashboardCd3LowImmuneThreshold;
            row.DashboardCd3OptimalMin = request.DashboardCd3OptimalMin;
            row.DashboardCd3OptimalMax = request.DashboardCd3OptimalMax;
        }
    }
}
