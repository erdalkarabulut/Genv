using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;

public class Tank : Entity<Guid>
{
    public string Name { get; set; } = default!;

    public virtual ICollection<Rack> Racks { get; set; }

    public Tank()
    {
        Racks = new HashSet<Rack>();
    }

    public int GetTotalBagCellCount()
    {
        return Racks?.Sum(r => r.GetTotalBagCellCount()) ?? 0;
    }

    public int GetOccupiedBagCellCount()
    {
        return Racks?.Sum(r => r.GetOccupiedBagCellCount()) ?? 0;
    }
}
