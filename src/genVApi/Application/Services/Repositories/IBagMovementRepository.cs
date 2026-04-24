using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;

namespace Application.Services.Repositories;

public interface IBagMovementRepository : IAsyncRepository<BagMovement, Guid>, IRepository<BagMovement, Guid>
{
}