using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class RackRepository : EfRepositoryBase<Rack, Guid, BaseDbContext>, IRackRepository
{
    public RackRepository(BaseDbContext context) : base(context)
    {
    }
}