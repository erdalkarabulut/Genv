using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;

namespace Application.Services.Repositories;

public interface IRackRepository : IAsyncRepository<Rack, Guid>, IRepository<Rack, Guid>
{
}