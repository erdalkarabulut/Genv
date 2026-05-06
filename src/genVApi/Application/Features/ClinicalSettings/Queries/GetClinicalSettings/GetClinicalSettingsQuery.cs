using Application.Features.ClinicalConfiguration;
using Application.Features.ClinicalConfiguration.Constants;
using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using static Application.Features.ClinicalConfiguration.Constants.ClinicalSettingsOperationClaims;

namespace Application.Features.ClinicalConfiguration.Queries.GetClinicalSettings;

public class GetClinicalSettingsQuery : IRequest<ClinicalSettingsDto>, ISecuredRequest
{
    /// <summary>Admin veya ClinicalSettings okuma yetkisi olan kullanıcılar.</summary>
    public string[] Roles => [Admin, Read];

    public class Handler : IRequestHandler<GetClinicalSettingsQuery, ClinicalSettingsDto>
    {
        private readonly IClinicalSettingsRepository _repository;

        public Handler(IClinicalSettingsRepository repository) => _repository = repository;

        public async Task<ClinicalSettingsDto> Handle(GetClinicalSettingsQuery request, CancellationToken cancellationToken)
        {
            ClinicalSettings? row = await _repository.GetAsync(
                predicate: x => x.Id == ClinicalSettings.SingletonId,
                cancellationToken: cancellationToken);

            if (row is null)
            {
                return new ClinicalSettingsDto
                {
                    Id = ClinicalSettings.SingletonId,
                    SessionCd34Cd3Divisor = 10000,
                    DliCd3CalculationDivisor = 10000,
                    TargetCd34AutologousPerKg = 2,
                    TargetCd34AllogeneicPerKg = 4,
                    IdealCd34AutologousPerKg = 4,
                    IdealCd34AllogeneicPerKg = 5,
                    MaxApheresisDaysAutologous = 4,
                    MaxApheresisDaysAllogeneic = 2,
                    DliHighDoseCd3PerKgThreshold = 10,
                    ProductMinimumCd34PerKg = 2,
                    DashboardCd34LowThreshold = 2,
                    DashboardCd34ElevatedFloor = 4,
                    DashboardCd3HighRiskThreshold = 10,
                    DashboardCd3LowImmuneThreshold = 2,
                    DashboardCd3OptimalMin = 3,
                    DashboardCd3OptimalMax = 8
                };
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
    }
}
