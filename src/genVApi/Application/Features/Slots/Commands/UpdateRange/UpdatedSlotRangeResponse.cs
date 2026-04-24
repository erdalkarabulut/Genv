using NArchitecture.Core.Application.Responses;

namespace Application.Features.Slots.Commands.UpdateRange;

public class UpdatedSlotRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
