using NArchitecture.Core.Application.Responses;

namespace Application.Features.Donors.Commands.DeleteRange;

public class DeletedDonorRangeResponse : IResponse
{
    public int DeletedCount { get; set; }
}
