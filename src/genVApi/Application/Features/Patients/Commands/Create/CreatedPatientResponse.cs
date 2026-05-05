using NArchitecture.Core.Application.Responses;

namespace Application.Features.Patients.Commands.Create;

public class CreatedPatientResponse : IResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public double WeightKg { get; set; }
    public string? BloodGroup { get; set; }
    public Domain.Enums.TransplantType TransplantType { get; set; }
    public string? Diagnosis { get; set; }
    public string? ProtocolNo { get; set; }
    public string? IdentityNumber { get; set; }
    public DateTime? BirthDate { get; set; }
    public Guid? DonorId { get; set; }
}