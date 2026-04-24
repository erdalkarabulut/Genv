using NArchitecture.Core.Application.Responses;

namespace Application.Features.DliProducts.Commands.UpdateRange;

public class UpdatedDliProductRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
