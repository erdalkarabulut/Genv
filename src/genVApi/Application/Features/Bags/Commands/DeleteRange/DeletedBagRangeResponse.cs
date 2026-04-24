using NArchitecture.Core.Application.Responses;

namespace Application.Features.Bags.Commands.DeleteRange;

public class DeletedBagRangeResponse : IResponse
{
    public int DeletedCount { get; set; }
}
