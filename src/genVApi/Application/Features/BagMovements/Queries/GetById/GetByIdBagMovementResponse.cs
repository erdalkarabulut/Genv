using NArchitecture.Core.Application.Responses;

namespace Application.Features.BagMovements.Queries.GetById;

public class GetByIdBagMovementResponse : IResponse
{
    public Guid Id { get; set; }
    public Guid BagId { get; set; }
    public Guid? FromBagCellId { get; set; }
    public Guid? ToBagCellId { get; set; }
    public string Action { get; set; }
    public string? ActionDisplay { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? UsedAt { get; set; }

    // Patient info (if available via bag -> session -> patient)
    public Guid? PatientId { get; set; }
    public string? PatientFullName { get; set; }
}