using NArchitecture.Core.Application.Responses;

namespace Application.Features.Products.Commands.UpdateRange;

public class UpdatedProductRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
