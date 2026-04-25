using System.Collections.Generic;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.CollectionSessions;

public interface ICollectionSessionService
{
    Task<CollectionSession?> GetAsync(
        Expression<Func<CollectionSession, bool>> predicate,
        Func<IQueryable<CollectionSession>, IIncludableQueryable<CollectionSession, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<IPaginate<CollectionSession>?> GetListAsync(
        Expression<Func<CollectionSession, bool>>? predicate = null,
        Func<IQueryable<CollectionSession>, IOrderedQueryable<CollectionSession>>? orderBy = null,
        Func<IQueryable<CollectionSession>, IIncludableQueryable<CollectionSession, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<CollectionSession> AddAsync(CollectionSession collectionSession);
    Task<ICollection<CollectionSession>> AddRangeAsync(ICollection<CollectionSession> collectionSessions);
    Task<CollectionSession> UpdateAsync(CollectionSession collectionSession);
    Task<ICollection<CollectionSession>> UpdateRangeAsync(ICollection<CollectionSession> collectionSessions);
    Task<CollectionSession> DeleteAsync(CollectionSession collectionSession, bool permanent = true);
    Task<ICollection<CollectionSession>> DeleteRangeAsync(ICollection<CollectionSession> collectionSessions, bool permanent = true);
}
