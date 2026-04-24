using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class CollectionSessionRepository : EfRepositoryBase<CollectionSession, Guid, BaseDbContext>, ICollectionSessionRepository
{
    public CollectionSessionRepository(BaseDbContext context) : base(context)
    {
    }
}