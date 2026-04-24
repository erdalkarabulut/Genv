using Application.Features.DliProducts.Constants;
using Application.Features.DliProducts.Rules;
using Application.Services.ClinicalConfiguration;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.DliProducts.Constants.DliProductsOperationClaims;

namespace Application.Features.DliProducts.Commands.Create;

public class CreateDliProductCommand : IRequest<CreatedDliProductResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid PatientId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid? DonorId { get; set; }
    public DateTime? Date { get; set; }
    public double VolumeMl { get; set; }
    public double? Wbc { get; set; }
    public double? LymphocytePercent { get; set; }
    public double? Cd3Percent { get; set; }
    /// <summary>Manuel override — boş bırakılırsa otomatik hesaplanır.</summary>
    public double? Cd3PerKgOverride { get; set; }
    public string? Notes { get; set; }

    public string[] Roles => [Admin, Write, DliProductsOperationClaims.Create];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetDliProducts"];

    public class CreateDliProductCommandHandler : IRequestHandler<CreateDliProductCommand, CreatedDliProductResponse>
    {
        private readonly IMapper _mapper;
        private readonly IDliProductRepository _dliProductRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly DliProductBusinessRules _dliProductBusinessRules;
        private readonly IClinicalThresholdsAccessor _clinicalThresholdsAccessor;

        public CreateDliProductCommandHandler(IMapper mapper, IDliProductRepository dliProductRepository,
                                         IPatientRepository patientRepository,
                                         DliProductBusinessRules dliProductBusinessRules,
                                         IClinicalThresholdsAccessor clinicalThresholdsAccessor)
        {
            _mapper = mapper;
            _dliProductRepository = dliProductRepository;
            _patientRepository = patientRepository;
            _dliProductBusinessRules = dliProductBusinessRules;
            _clinicalThresholdsAccessor = clinicalThresholdsAccessor;
        }

        public async Task<CreatedDliProductResponse> Handle(CreateDliProductCommand request, CancellationToken cancellationToken)
        {
            DliProduct dliProduct = _mapper.Map<DliProduct>(request);
            if (request.Date.HasValue) dliProduct.Date = request.Date.Value;
            else if (dliProduct.Date == default) dliProduct.Date = DateTime.UtcNow;

            // Otomatik CD3/kg hesabı (override yoksa).
            Patient? patient = await _patientRepository.GetAsync(
                predicate: p => p.Id == request.PatientId,
                enableTracking: false,
                cancellationToken: cancellationToken);
            if (patient is not null && patient.WeightKg > 0)
            {
                var t = await _clinicalThresholdsAccessor.GetAsync(cancellationToken);
                if (request.Cd3PerKgOverride.HasValue)
                    dliProduct.Cd3PerKg = request.Cd3PerKgOverride.Value;
                else
                    dliProduct.Calculate(patient.WeightKg, t.DliCd3CalculationDivisor);
            }

            await _dliProductRepository.AddAsync(dliProduct);

            CreatedDliProductResponse response = _mapper.Map<CreatedDliProductResponse>(dliProduct);
            return response;
        }
    }
}