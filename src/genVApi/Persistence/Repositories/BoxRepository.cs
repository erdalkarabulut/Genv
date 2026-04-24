using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class BoxRepository : EfRepositoryBase<Box, Guid, BaseDbContext>, IBoxRepository
{
    public BoxRepository(BaseDbContext context) : base(context)
    {
    }
}