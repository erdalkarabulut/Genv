using NArchitecture.Core.Application.Responses;

namespace Application.Features.Boxes.Commands.DeleteRange;

public class DeletedBoxRangeResponse : IResponse
{
    public int DeletedCount { get; set; }
}
