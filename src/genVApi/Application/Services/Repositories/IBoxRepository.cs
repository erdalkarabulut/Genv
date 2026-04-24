using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;

namespace Application.Services.Repositories;

public interface IBoxRepository : IAsyncRepository<Box, Guid>, IRepository<Box, Guid>
{
}