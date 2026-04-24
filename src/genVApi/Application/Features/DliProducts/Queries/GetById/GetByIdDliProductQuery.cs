using Application.Features.DliProducts.Constants;
using Application.Features.DliProducts.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.DliProducts.Constants.DliProductsOperationClaims;

namespace Application.Features.DliProducts.Queries.GetById;

public class GetByIdDliProductQuery : IRequest<GetByIdDliProductResponse>, ISecuredRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetByIdDliProductQueryHandler : IRequestHandler<GetByIdDliProductQuery, GetByIdDliProductResponse>
    {
        private readonly IMapper _mapper;
        private readonly IDliProductRepository _dliProductRepository;
        private readonly DliProductBusinessRules _dliProductBusinessRules;

        public GetByIdDliProductQueryHandler(IMapper mapper, IDliProductRepository dliProductRepository, DliProductBusinessRules dliProductBusinessRules)
        {
            _mapper = mapper;
            _dliProductRepository = dliProductRepository;
            _dliProductBusinessRules = dliProductBusinessRules;
        }

        public async Task<GetByIdDliProductResponse> Handle(GetByIdDliProductQuery request, CancellationToken cancellationToken)
        {
            DliProduct? dliProduct = await _dliProductRepository.GetAsync(predicate: dp => dp.Id == request.Id, cancellationToken: cancellationToken);
            await _dliProductBusinessRules.DliProductShouldExistWhenSelected(dliProduct);

            GetByIdDliProductResponse response = _mapper.Map<GetByIdDliProductResponse>(dliProduct);
            return response;
        }
    }
}