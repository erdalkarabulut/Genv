using NArchitecture.Core.Application.Responses;

namespace Application.Features.DliProducts.Commands.CreateRange;

public class CreatedDliProductRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
