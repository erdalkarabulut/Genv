using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;

namespace Application.Services.Repositories;

public interface IDliProductRepository : IAsyncRepository<DliProduct, Guid>, IRepository<DliProduct, Guid>
{
}