using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;

namespace Application.Services.Repositories;

public interface IBagRepository : IAsyncRepository<Bag, Guid>, IRepository<Bag, Guid>
{
}