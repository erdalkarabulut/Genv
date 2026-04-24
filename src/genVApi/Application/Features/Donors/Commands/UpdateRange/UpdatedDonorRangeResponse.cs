using NArchitecture.Core.Application.Responses;

namespace Application.Features.Donors.Commands.UpdateRange;

public class UpdatedDonorRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
