using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;

namespace Application.Services.Repositories;

public interface IPlcAlarmContactRepository : IAsyncRepository<PlcAlarmContact, Guid>, IRepository<PlcAlarmContact, Guid>
{
}
