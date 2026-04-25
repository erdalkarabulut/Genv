using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;

public class Box : Entity<Guid>
{
    public Guid SlotId { get; set; }
    public string Name { get; set; } = default!;

    public virtual Slot Slot { get; set; } = default!;
    public virtual ICollection<BagCell> BagCells { get; set; }

    public Box()
    {
        BagCells = new HashSet<BagCell>();
    }

    public int GetEmptyBagCellCount()
    {
        return BagCells?.Count(x => !x.IsOccupied) ?? 0;
    }
}
