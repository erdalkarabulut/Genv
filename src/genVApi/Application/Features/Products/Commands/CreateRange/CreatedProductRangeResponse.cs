using NArchitecture.Core.Application.Responses;

namespace Application.Features.Products.Commands.CreateRange;

public class CreatedProductRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
