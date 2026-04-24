using NArchitecture.Core.Application.Responses;

namespace Application.Features.Patients.Commands.DeleteRange;

public class DeletedPatientRangeResponse : IResponse
{
    public int DeletedCount { get; set; }
}
