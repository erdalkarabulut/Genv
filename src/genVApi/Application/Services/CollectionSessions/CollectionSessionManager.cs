using System.Collections.Generic;
using Application.Features.CollectionSessions.Rules;
using Application.Services.Repositories;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.CollectionSessions;

public class CollectionSessionManager : ICollectionSessionService
{
    private readonly ICollectionSessionRepository _collectionSessionRepository;
    private readonly CollectionSessionBusinessRules _collectionSessionBusinessRules;

    public CollectionSessionManager(ICollectionSessionRepository collectionSessionRepository, CollectionSessionBusinessRules collectionSessionBusinessRules)
    {
        _collectionSessionRepository = collectionSessionRepository;
        _collectionSessionBusinessRules = collectionSessionBusinessRules;
    }

    public async Task<CollectionSession?> GetAsync(
        Expression<Func<CollectionSession, bool>> predicate,
        Func<IQueryable<CollectionSession>, IIncludableQueryable<CollectionSession, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        CollectionSession? collectionSession = await _collectionSessionRepository.GetAsync(predicate, include, withDeleted, enableTracking, cancellationToken);
        return collectionSession;
    }

    public async Task<IPaginate<CollectionSession>?> GetListAsync(
        Expression<Func<CollectionSession, bool>>? predicate = null,
        Func<IQueryable<CollectionSession>, IOrderedQueryable<CollectionSession>>? orderBy = null,
        Func<IQueryable<CollectionSession>, IIncludableQueryable<CollectionSession, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        IPaginate<CollectionSession> collectionSessionList = await _collectionSessionRepository.GetListAsync(
            predicate,
            orderBy,
            include,
            index,
            size,
            withDeleted,
            enableTracking,
            cancellationToken
        );
        return collectionSessionList;
    }

    public async Task<CollectionSession> AddAsync(CollectionSession collectionSession)
    {
        CollectionSession addedCollectionSession = await _collectionSessionRepository.AddAsync(collectionSession);

        return addedCollectionSession;
    }

    public async Task<ICollection<CollectionSession>> AddRangeAsync(ICollection<CollectionSession> collectionSessions)
    {
        ICollection<CollectionSession> addedCollectionSessions = await _collectionSessionRepository.AddRangeAsync(collectionSessions);

        return addedCollectionSessions;
    }

    public async Task<CollectionSession> UpdateAsync(CollectionSession collectionSession)
    {
        CollectionSession updatedCollectionSession = await _collectionSessionRepository.UpdateAsync(collectionSession);

        return updatedCollectionSession;
    }

    public async Task<ICollection<CollectionSession>> UpdateRangeAsync(ICollection<CollectionSession> collectionSessions)
    {
        ICollection<CollectionSession> updatedCollectionSessions = await _collectionSessionRepository.UpdateRangeAsync(collectionSessions);

        return updatedCollectionSessions;
    }

    public async Task<CollectionSession> DeleteAsync(CollectionSession collectionSession, bool permanent = true)
    {
        CollectionSession deletedCollectionSession = await _collectionSessionRepository.DeleteAsync(collectionSession, permanent);

        return deletedCollectionSession;
    }

    public async Task<ICollection<CollectionSession>> DeleteRangeAsync(ICollection<CollectionSession> collectionSessions, bool permanent = true)
    {
        ICollection<CollectionSession> deletedCollectionSessions = await _collectionSessionRepository.DeleteRangeAsync(collectionSessions, permanent);

        return deletedCollectionSessions;
    }
}
