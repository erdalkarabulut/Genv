using NArchitecture.Core.Application.Dtos;

namespace Application.Features.BagMovements.Queries.GetList;

public class GetListBagMovementListItemDto : IDto
{
    public Guid Id { get; set; }
    public Guid BagId { get; set; }
    public Guid? FromBagCellId { get; set; }
    public Guid? ToBagCellId { get; set; }

    // Full location string for from/to bag cells (e.g. "Tank1-RackA-Slot1-Box1-A1")
    public string? FromBagCellLocation { get; set; }
    public string? ToBagCellLocation { get; set; }

    public string Action { get; set; }
    // Friendly display for action (e.g. "Use (Infusion)")
    public string? ActionDisplay { get; set; }
    public DateTime CreatedDate { get; set; }

    // If applicable (when bag is used), the exact used timestamp.
    public DateTime? UsedAt { get; set; }

    // Patient snapshot for reporting/search
    public Guid? PatientId { get; set; }
    public string? PatientFullName { get; set; }
}