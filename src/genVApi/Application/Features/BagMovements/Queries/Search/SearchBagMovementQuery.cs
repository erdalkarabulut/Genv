using System.Linq.Expressions;
using Application.Features.BagMovements.Constants;
using Application.Features.BagMovements.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;
using MediatR;
using static Application.Features.BagMovements.Constants.BagMovementsOperationClaims;

namespace Application.Features.BagMovements.Queries.Search;

/// <summary>
/// Searches bag movements by action, bagId, or patient full name.
/// Uses pure LINQ — no dynamic-linq.
/// </summary>
public class SearchBagMovementQuery : IRequest<GetListResponse<GetListBagMovementListItemDto>>, ISecuredRequest, ICachableRequest
{
    public PageRequest PageRequest { get; set; }

    /// <summary>
    /// Free-text search across action, bagId, and patient full name.
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// Optional: filter by specific action (e.g. "BagStored", "BagUsed").
    /// </summary>
    public string? Action { get; set; }

    public string[] Roles => [Admin, Read];

    public bool BypassCache { get; }
    public string? CacheKey => $"SearchBagMovements({PageRequest.PageIndex},{PageRequest.PageSize},{SearchText},{Action})";
    public string? CacheGroupKey => "GetBagMovements";
    public TimeSpan? SlidingExpiration { get; }

    public class SearchBagMovementQueryHandler : IRequestHandler<SearchBagMovementQuery, GetListResponse<GetListBagMovementListItemDto>>
    {
        private readonly IBagMovementRepository _bagMovementRepository;
        private readonly IMapper _mapper;

        public SearchBagMovementQueryHandler(IBagMovementRepository bagMovementRepository, IMapper _mapper)
        {
            _bagMovementRepository = bagMovementRepository;
            this._mapper = _mapper;
        }

        public async Task<GetListResponse<GetListBagMovementListItemDto>> Handle(SearchBagMovementQuery request, CancellationToken cancellationToken)
        {
            string? search = string.IsNullOrWhiteSpace(request.SearchText) ? null : request.SearchText.Trim().ToLower();
            string? actionFilter = string.IsNullOrWhiteSpace(request.Action) ? null : request.Action;

            // ILike for action field (guaranteed PostgreSQL translation)
            // Use Contains only when there's an actual search term
            Expression<Func<BagMovement, bool>>? searchPredicate = string.IsNullOrEmpty(search)
                ? null
                : bm =>
                    EF.Functions.ILike(bm.Action, $"%{search}%")
                    || EF.Functions.ILike(bm.BagId.ToString(), $"%{search}%")
                    || (bm.Bag != null && bm.Bag.Session != null && bm.Bag.Session.Patient != null
                        && EF.Functions.ILike(bm.Bag.Session.Patient.FullName, $"%{search}%"));

            IPaginate<BagMovement> bagMovements;
            if (!string.IsNullOrEmpty(actionFilter))
            {
                // Action filter via dynamic-linq + sort
                DynamicQuery dynamicQuery = new()
                {
                    Filter = new Filter { Field = "action", Operator = "eq", Value = actionFilter },
                    Sort = new[] { new Sort { Field = "createdDate", Dir = "desc" } }
                };
                bagMovements = await _bagMovementRepository.GetListByDynamicAsync(
                    dynamicQuery,
                    predicate: searchPredicate,
                    include: m => m.Include(bm => bm.Bag).ThenInclude(b => b.Session).ThenInclude(s => s.Patient)
                               .Include(bm => bm.FromBagCell).ThenInclude(fbc => fbc!.Box).ThenInclude(b => b.Slot).ThenInclude(s => s.Rack).ThenInclude(r => r.Tank)
                               .Include(bm => bm.ToBagCell).ThenInclude(tbc => tbc!.Box).ThenInclude(b => b.Slot).ThenInclude(s => s.Rack).ThenInclude(r => r.Tank),
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    cancellationToken: cancellationToken,
                    enableTracking: false
                );
            }
            else
            {
                // Pure LINQ with ILike — orderBy via GetListAsync
                bagMovements = await _bagMovementRepository.GetListAsync(
                    predicate: searchPredicate,
                    orderBy: bm => bm.OrderByDescending(x => x.CreatedDate),
                    include: m => m.Include(bm => bm.Bag).ThenInclude(b => b.Session).ThenInclude(s => s.Patient)
                               .Include(bm => bm.FromBagCell).ThenInclude(fbc => fbc!.Box).ThenInclude(b => b.Slot).ThenInclude(s => s.Rack).ThenInclude(r => r.Tank)
                               .Include(bm => bm.ToBagCell).ThenInclude(tbc => tbc!.Box).ThenInclude(b => b.Slot).ThenInclude(s => s.Rack).ThenInclude(r => r.Tank),
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    cancellationToken: cancellationToken,
                    enableTracking: false
                );
            }

            GetListResponse<GetListBagMovementListItemDto> response = _mapper.Map<GetListResponse<GetListBagMovementListItemDto>>(bagMovements);
            return response;
        }
    }
}
