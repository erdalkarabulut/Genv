using NArchitecture.Core.Application.Responses;

namespace Application.Features.Patients.Commands.UpdateRange;

public class UpdatedPatientRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
