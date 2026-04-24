using Application.Features.Boxes.Constants;
using Application.Features.Boxes.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Boxes.Constants.BoxesOperationClaims;

namespace Application.Features.Boxes.Commands.UpdateRange;

public class UpdateBoxRangeCommand : IRequest<UpdatedBoxRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<UpdateBoxRangeItem> Items { get; set; } = new List<UpdateBoxRangeItem>();

    public string[] Roles => [Admin, Write, BoxesOperationClaims.UpdateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBoxes"];

    public class UpdateBoxRangeItem
    {
        public Guid Id { get; set; }
        public Guid RackId { get; set; }
        public string Name { get; set; }
    }

    public class UpdateBoxRangeCommandHandler : IRequestHandler<UpdateBoxRangeCommand, UpdatedBoxRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBoxRepository _boxRepository;
        private readonly BoxBusinessRules _boxBusinessRules;

        public UpdateBoxRangeCommandHandler(IMapper mapper, IBoxRepository boxRepository, BoxBusinessRules boxBusinessRules)
        {
            _mapper = mapper;
            _boxRepository = boxRepository;
            _boxBusinessRules = boxBusinessRules;
        }

        public async Task<UpdatedBoxRangeResponse> Handle(UpdateBoxRangeCommand request, CancellationToken cancellationToken)
        {
            List<Box> items = new List<Box>();

            foreach (UpdateBoxRangeItem item in request.Items)
            {
                Box? entity = await _boxRepository.GetAsync(
                    predicate: x => x.Id == item.Id,
                    cancellationToken: cancellationToken
                );
                await _boxBusinessRules.BoxShouldExistWhenSelected(entity);
                _mapper.Map(source: item, destination: entity!);
                items.Add(entity!);
            }

            ICollection<Box> updated = await _boxRepository.UpdateRangeAsync(items);

            return new UpdatedBoxRangeResponse { Ids = updated.Select(e => e.Id).ToList() };
        }
    }
}