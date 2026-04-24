using NArchitecture.Core.Application.Responses;

namespace Application.Features.DliProducts.Commands.DeleteRange;

public class DeletedDliProductRangeResponse : IResponse
{
    public int DeletedCount { get; set; }
}
