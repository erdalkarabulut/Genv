using NArchitecture.Core.Application.Responses;

namespace Application.Features.Donors.Commands.CreateRange;

public class CreatedDonorRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
