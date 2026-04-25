using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;

namespace Application.Services.Repositories;

public interface IBagCellRepository : IAsyncRepository<BagCell, Guid>, IRepository<BagCell, Guid>
{
}
