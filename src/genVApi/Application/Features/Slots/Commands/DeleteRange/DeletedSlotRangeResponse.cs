using NArchitecture.Core.Application.Responses;

namespace Application.Features.Slots.Commands.DeleteRange;

public class DeletedSlotRangeResponse : IResponse
{
    public int DeletedCount { get; set; }
}
