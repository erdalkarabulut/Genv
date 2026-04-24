using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;

namespace Application.Services.Repositories;

public interface ISlotRepository : IAsyncRepository<Slot, Guid>, IRepository<Slot, Guid>
{
}