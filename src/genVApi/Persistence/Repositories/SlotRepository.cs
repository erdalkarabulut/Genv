using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class SlotRepository : EfRepositoryBase<Slot, Guid, BaseDbContext>, ISlotRepository
{
    public SlotRepository(BaseDbContext context) : base(context)
    {
    }
}