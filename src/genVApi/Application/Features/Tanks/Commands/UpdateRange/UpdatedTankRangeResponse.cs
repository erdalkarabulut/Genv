using NArchitecture.Core.Application.Responses;

namespace Application.Features.Tanks.Commands.UpdateRange;

public class UpdatedTankRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
