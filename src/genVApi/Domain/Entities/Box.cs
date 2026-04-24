using NArchitecture.Core.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;
public class Box : Entity<Guid>
{
    public Guid RackId { get; set; }
    public string Name { get; set; }

    public virtual Rack Rack { get; set; }
    public virtual ICollection<Slot> Slots { get; set; }

    public Box()
    {
        Slots = new HashSet<Slot>();
    }

    public int GetEmptySlotCount()
    {
        return Slots?.Count(x => !x.IsOccupied) ?? 0;
    }
}
