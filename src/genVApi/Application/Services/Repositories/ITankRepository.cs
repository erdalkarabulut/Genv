using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;

namespace Application.Services.Repositories;

public interface ITankRepository : IAsyncRepository<Tank, Guid>, IRepository<Tank, Guid>
{
}