using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class DliProductRepository : EfRepositoryBase<DliProduct, Guid, BaseDbContext>, IDliProductRepository
{
    public DliProductRepository(BaseDbContext context) : base(context)
    {
    }
}