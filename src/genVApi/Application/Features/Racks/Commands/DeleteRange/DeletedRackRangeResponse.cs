using NArchitecture.Core.Application.Responses;

namespace Application.Features.Racks.Commands.DeleteRange;

public class DeletedRackRangeResponse : IResponse
{
    public int DeletedCount { get; set; }
}
