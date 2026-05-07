using System.Linq.Expressions;
using System.Reflection;
using Application.Features.BagMovements.Constants;
using Application.Features.BagMovements.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.BagMovements.Constants.BagMovementsOperationClaims;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.BagMovements.Queries.GetListByDynamic;

public class GetListByDynamicBagMovementQuery : IRequest<GetListResponse<GetListBagMovementListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetListByDynamicBagMovementQueryHandler : IRequestHandler<GetListByDynamicBagMovementQuery, GetListResponse<GetListBagMovementListItemDto>>
    {
        private readonly IBagMovementRepository _bagMovementRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicBagMovementQueryHandler(IBagMovementRepository bagMovementRepository, IMapper mapper)
        {
            _bagMovementRepository = bagMovementRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListBagMovementListItemDto>> Handle(GetListByDynamicBagMovementQuery request, CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            // Extract and remove patient-related filters (e.g. "bag.session.patient.fullName")
            var patientFilters = new List<Filter>();
            if (dynamicQuery.Filter != null)
            {
                Filter? cleaned = RemovePatientFilters(dynamicQuery.Filter, patientFilters);
                dynamicQuery.Filter = cleaned;
            }

            // Build typed EF predicate from patient filters (OR-combined)
            Expression<Func<BagMovement, bool>>? patientPredicate = null;
            if (patientFilters.Count != 0)
            {
                patientPredicate = BuildPatientPredicate(patientFilters);
            }

            IPaginate<BagMovement> bagMovements = await _bagMovementRepository.GetListByDynamicAsync(
                dynamicQuery,
                predicate: patientPredicate,
                include: m => m.Include(bm => bm.Bag).ThenInclude(b => b.Session).ThenInclude(s => s.Patient)
                           .Include(bm => bm.FromBagCell).ThenInclude(fbc => fbc!.Box).ThenInclude(b => b.Slot).ThenInclude(s => s.Rack).ThenInclude(r => r.Tank)
                           .Include(bm => bm.ToBagCell).ThenInclude(tbc => tbc!.Box).ThenInclude(b => b.Slot).ThenInclude(s => s.Rack).ThenInclude(r => r.Tank),
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            GetListResponse<GetListBagMovementListItemDto> response = _mapper.Map<GetListResponse<GetListBagMovementListItemDto>>(bagMovements);
            return response;

            // --- local helpers ---

            static Filter? RemovePatientFilters(Filter filter, List<Filter> collected)
            {
                if (filter.Filters is not null && filter.Filters.Any())
                {
                    var newChildren = new List<Filter>();
                    foreach (var child in filter.Filters)
                    {
                        var cleaned = RemovePatientFilters(child, collected);
                        if (cleaned != null)
                            newChildren.Add(cleaned);
                    }
                    if (newChildren.Count == 0) return null;
                    filter.Filters = newChildren;
                    return filter;
                }

                if (IsPatientField(filter.Field))
                {
                    collected.Add(filter);
                    return null;
                }
                return filter;
            }

            static bool IsPatientField(string? field)
            {
                if (string.IsNullOrWhiteSpace(field)) return false;
                string f = field.Trim().ToLowerInvariant();
                return f == "bag.session.patient.fullname" || f.StartsWith("bag.session.patient.");
            }

            static Expression<Func<BagMovement, bool>> BuildPatientPredicate(List<Filter> filters)
            {
                ParameterExpression bm = Expression.Parameter(typeof(BagMovement), "bm");
                Expression bagExpr = Expression.Property(bm, nameof(BagMovement.Bag));
                Expression sessionExpr = Expression.Property(bagExpr, nameof(Bag.Session));
                Expression patientExpr = Expression.Property(sessionExpr, nameof(CollectionSession.Patient));
                Expression fullNameExpr = Expression.Property(patientExpr, nameof(Patient.FullName));

                var nullChecks = Expression.AndAlso(
                    Expression.AndAlso(
                        Expression.NotEqual(bagExpr, Expression.Constant(null, typeof(Bag))),
                        Expression.NotEqual(sessionExpr, Expression.Constant(null, typeof(CollectionSession)))),
                    Expression.NotEqual(patientExpr, Expression.Constant(null, typeof(Patient))));

                var conds = new List<Expression>();
                MethodInfo? toLower = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes);
                foreach (var f in filters)
                {
                    string op = (f.Operator ?? "").ToLowerInvariant();
                    string val = ((f.Value ?? "").ToString()).ToLowerInvariant();
                    Expression valueConst = Expression.Constant(val);

                    Expression nameLower = toLower != null
                        ? Expression.Call(fullNameExpr, toLower)
                        : fullNameExpr;

                    Expression match = op switch
                    {
                        "contains" or "doesnotcontain" => Expression.Call(nameLower, nameof(string.Contains), new[] { typeof(string) }, valueConst),
                        "startswith" => Expression.Call(nameLower, nameof(string.StartsWith), new[] { typeof(string) }, valueConst),
                        "endswith" => Expression.Call(nameLower, nameof(string.EndsWith), new[] { typeof(string) }, valueConst),
                        "eq" => Expression.Equal(nameLower, valueConst),
                        "neq" => Expression.NotEqual(nameLower, valueConst),
                        _ => null!
                    };

                    if (match == null) continue;
                    if (op == "doesnotcontain") match = Expression.Not(match);
                    conds.Add(Expression.AndAlso(nullChecks, match));
                }

                if (conds.Count == 0) return _ => true;
                Expression body = conds.Count == 1 ? conds[0] : conds.Aggregate(Expression.OrElse);
                return Expression.Lambda<Func<BagMovement, bool>>(body, bm);
            }
        }
    }
}
