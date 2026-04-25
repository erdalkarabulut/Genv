using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class BagCellRepository : EfRepositoryBase<BagCell, Guid, BaseDbContext>, IBagCellRepository
{
    public BagCellRepository(BaseDbContext context)
        : base(context)
    {
    }
}
