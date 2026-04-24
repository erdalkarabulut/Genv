using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class DonorRepository : EfRepositoryBase<Donor, Guid, BaseDbContext>, IDonorRepository
{
    public DonorRepository(BaseDbContext context) : base(context)
    {
    }
}