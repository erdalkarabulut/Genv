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

namespace Application.Features.DliProducts.Commands.Update;

public class UpdateDliProductCommand : IRequest<UpdatedDliProductResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid? DonorId { get; set; }
    public DateTime? Date { get; set; }
    public double VolumeMl { get; set; }
    public double? Wbc { get; set; }
    public double? LymphocytePercent { get; set; }
    public double? Cd3Percent { get; set; }
    public double? Cd3PerKgOverride { get; set; }
    public string? Notes { get; set; }

    public string[] Roles => [Admin, Write, DliProductsOperationClaims.Update];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetDliProducts"];

    public class UpdateDliProductCommandHandler : IRequestHandler<UpdateDliProductCommand, UpdatedDliProductResponse>
    {
        private readonly IMapper _mapper;
        private readonly IDliProductRepository _dliProductRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly DliProductBusinessRules _dliProductBusinessRules;
        private readonly IClinicalThresholdsAccessor _clinicalThresholdsAccessor;

        public UpdateDliProductCommandHandler(IMapper mapper, IDliProductRepository dliProductRepository,
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

        public async Task<UpdatedDliProductResponse> Handle(UpdateDliProductCommand request, CancellationToken cancellationToken)
        {
            DliProduct? dliProduct = await _dliProductRepository.GetAsync(predicate: x => x.Id == request.Id, cancellationToken: cancellationToken);
            await _dliProductBusinessRules.DliProductShouldExistWhenSelected(dliProduct);
            dliProduct = _mapper.Map(request, dliProduct);
            if (request.Date.HasValue) dliProduct!.Date = request.Date.Value;

            Patient? patient = await _patientRepository.GetAsync(
                predicate: p => p.Id == request.PatientId,
                enableTracking: false,
                cancellationToken: cancellationToken);
            if (patient is not null && patient.WeightKg > 0)
            {
                var t = await _clinicalThresholdsAccessor.GetAsync(cancellationToken);
                if (request.Cd3PerKgOverride.HasValue)
                    dliProduct!.Cd3PerKg = request.Cd3PerKgOverride.Value;
                else
                    dliProduct!.Calculate(patient.WeightKg, t.DliCd3CalculationDivisor);
            }

            await _dliProductRepository.UpdateAsync(dliProduct!);

            UpdatedDliProductResponse response = _mapper.Map<UpdatedDliProductResponse>(dliProduct);
            return response;
        }
    }
}