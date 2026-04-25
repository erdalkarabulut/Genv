using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;

public class Rack : Entity<Guid>
{
    public Guid TankId { get; set; }
    public string Name { get; set; } = default!;

    public virtual Tank Tank { get; set; } = default!;
    public virtual ICollection<Slot> Slots { get; set; }

    public Rack()
    {
        Slots = new HashSet<Slot>();
    }

    public int GetTotalBagCellCount()
    {
        return Slots?.Sum(s => s.Boxes.Sum(b => b.BagCells.Count)) ?? 0;
    }

    public int GetOccupiedBagCellCount()
    {
        return Slots?.Sum(s => s.Boxes.Sum(b => b.BagCells.Count(c => c.IsOccupied))) ?? 0;
    }
}
