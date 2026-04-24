using NArchitecture.Core.Application.Responses;

namespace Application.Features.Tanks.Commands.DeleteRange;

public class DeletedTankRangeResponse : IResponse
{
    public int DeletedCount { get; set; }
}
