using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;

namespace Application.Services.Repositories;

public interface ICollectionSessionRepository : IAsyncRepository<CollectionSession, Guid>, IRepository<CollectionSession, Guid>
{
}