using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;

namespace Application.Services.Repositories;

public interface IDonorRepository : IAsyncRepository<Donor, Guid>, IRepository<Donor, Guid>
{
}