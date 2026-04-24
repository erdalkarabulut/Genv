using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class TankRepository : EfRepositoryBase<Tank, Guid, BaseDbContext>, ITankRepository
{
    public TankRepository(BaseDbContext context) : base(context)
    {
    }
}