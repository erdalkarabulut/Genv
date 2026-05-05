using NArchitecture.Core.Application.Responses;

namespace Application.Features.Donors.Commands.Update;

public class UpdatedDonorResponse : IResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public double WeightKg { get; set; }
    public string? BloodGroup { get; set; }
    public string? Relation { get; set; }
    public string? IdentityNumber { get; set; }
    public Domain.Enums.DonorType DonorType { get; set; }
    public DateTime? BirthDate { get; set; }
}