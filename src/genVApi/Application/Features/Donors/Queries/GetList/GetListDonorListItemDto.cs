using NArchitecture.Core.Application.Dtos;

namespace Application.Features.Donors.Queries.GetList;

public class GetListDonorListItemDto : IDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public double WeightKg { get; set; }
    public string? BloodGroup { get; set; }
    public string? Relation { get; set; }
    public string? IdentityNumber { get; set; }
    public Domain.Enums.DonorType DonorType { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime CreatedDate { get; set; }
}