using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;

public class BagMovement : Entity<Guid>
{
    public Guid BagId { get; set; }
    public Guid? FromBagCellId { get; set; }
    public Guid? ToBagCellId { get; set; }

    public string Action { get; set; } = default!;

    public virtual Bag Bag { get; set; } = default!;

    public string GetSummary()
    {
        return $"{Action} - {CreatedDate}";
    }
}
