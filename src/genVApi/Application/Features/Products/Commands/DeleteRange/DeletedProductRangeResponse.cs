using NArchitecture.Core.Application.Responses;

namespace Application.Features.Products.Commands.DeleteRange;

public class DeletedProductRangeResponse : IResponse
{
    public int DeletedCount { get; set; }
}
