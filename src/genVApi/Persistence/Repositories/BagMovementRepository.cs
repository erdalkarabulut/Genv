using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class BagMovementRepository : EfRepositoryBase<BagMovement, Guid, BaseDbContext>, IBagMovementRepository
{
    public BagMovementRepository(BaseDbContext context) : base(context)
    {
    }
}