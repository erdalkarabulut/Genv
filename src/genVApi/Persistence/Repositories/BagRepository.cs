using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class BagRepository : EfRepositoryBase<Bag, Guid, BaseDbContext>, IBagRepository
{
    public BagRepository(BaseDbContext context) : base(context)
    {
    }
}