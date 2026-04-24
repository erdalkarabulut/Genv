using NArchitecture.Core.Application.Responses;

namespace Application.Features.Tanks.Commands.CreateRange;

public class CreatedTankRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
