using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;

public class BagMovement : Entity<Guid>
{
    public Guid BagId { get; set; }
    public Guid? FromBagCellId { get; set; }
    public Guid? ToBagCellId { get; set; }

    public string Action { get; set; } = default!;

    // If the movement is a "Use" action, store when the bag was used.
    public DateTime? UsedAt { get; set; }

    public virtual Bag Bag { get; set; } = default!;
    public virtual BagCell? FromBagCell { get; set; }
    public virtual BagCell? ToBagCell { get; set; }

    public string GetSummary()
    {
        return $"{Action} - {CreatedDate}";
    }
}
