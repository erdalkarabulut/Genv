using Application.Features.DliProducts.Constants;
using Application.Features.DliProducts.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.DliProducts.Constants.DliProductsOperationClaims;

namespace Application.Features.DliProducts.Commands.UpdateRange;

public class UpdateDliProductRangeCommand : IRequest<UpdatedDliProductRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<UpdateDliProductRangeItem> Items { get; set; } = new List<UpdateDliProductRangeItem>();

    public string[] Roles => [Admin, Write, DliProductsOperationClaims.UpdateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetDliProducts"];

    public class UpdateDliProductRangeItem
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public double Cd3PerKg { get; set; }
        public double VolumeMl { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateDliProductRangeCommandHandler : IRequestHandler<UpdateDliProductRangeCommand, UpdatedDliProductRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IDliProductRepository _dliProductRepository;
        private readonly DliProductBusinessRules _dliProductBusinessRules;

        public UpdateDliProductRangeCommandHandler(IMapper mapper, IDliProductRepository dliProductRepository, DliProductBusinessRules dliProductBusinessRules)
        {
            _mapper = mapper;
            _dliProductRepository = dliProductRepository;
            _dliProductBusinessRules = dliProductBusinessRules;
        }

        public async Task<UpdatedDliProductRangeResponse> Handle(UpdateDliProductRangeCommand request, CancellationToken cancellationToken)
        {
            List<DliProduct> items = new List<DliProduct>();

            foreach (UpdateDliProductRangeItem item in request.Items)
            {
                DliProduct? entity = await _dliProductRepository.GetAsync(
                    predicate: x => x.Id == item.Id,
                    cancellationToken: cancellationToken
                );
                await _dliProductBusinessRules.DliProductShouldExistWhenSelected(entity);
                _mapper.Map(source: item, destination: entity!);
                items.Add(entity!);
            }

            ICollection<DliProduct> updated = await _dliProductRepository.UpdateRangeAsync(items);

            return new UpdatedDliProductRangeResponse { Ids = updated.Select(e => e.Id).ToList() };
        }
    }
}