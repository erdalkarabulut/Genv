using Application.Features.DliProducts.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.DliProducts.Constants.DliProductsOperationClaims;

namespace Application.Features.DliProducts.Commands.CreateRange;

public class CreateDliProductRangeCommand : IRequest<CreatedDliProductRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<CreateDliProductRangeItem> Items { get; set; } = new List<CreateDliProductRangeItem>();

    public string[] Roles => [Admin, Write, DliProductsOperationClaims.CreateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetDliProducts"];

    public class CreateDliProductRangeItem
    {
        public Guid PatientId { get; set; }
        public double Cd3PerKg { get; set; }
        public double VolumeMl { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateDliProductRangeCommandHandler : IRequestHandler<CreateDliProductRangeCommand, CreatedDliProductRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IDliProductRepository _dliProductRepository;

        public CreateDliProductRangeCommandHandler(IMapper mapper, IDliProductRepository dliProductRepository)
        {
            _mapper = mapper;
            _dliProductRepository = dliProductRepository;
        }

        public async Task<CreatedDliProductRangeResponse> Handle(CreateDliProductRangeCommand request, CancellationToken cancellationToken)
        {
            List<DliProduct> dliProducts = request.Items.Select(item => _mapper.Map<DliProduct>(item)).ToList();

            ICollection<DliProduct> addedDliProducts = await _dliProductRepository.AddRangeAsync(dliProducts);

            return new CreatedDliProductRangeResponse { Ids = addedDliProducts.Select(e => e.Id).ToList() };
        }
    }
}