using NArchitecture.Core.Application.Responses;

namespace Application.Features.Slots.Commands.CreateRange;

public class CreatedSlotRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
