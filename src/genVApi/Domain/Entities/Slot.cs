using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;

/// <summary>
/// Raf üzerindeki fiziksel slot (kutu oturur). Hiyerarşi: Tank → Rack → Slot → Box → BagCell.
/// </summary>
public class Slot : Entity<Guid>
{
    public Guid RackId { get; set; }
    public string Name { get; set; } = default!;

    public virtual Rack Rack { get; set; } = default!;
    public virtual ICollection<Box> Boxes { get; set; }

    public Slot()
    {
        Boxes = new HashSet<Box>();
    }
}
