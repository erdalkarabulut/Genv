using NArchitecture.Core.Application.Responses;

namespace Application.Features.Patients.Commands.CreateRange;

public class CreatedPatientRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
