using NArchitecture.Core.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;
public class Rack : Entity<Guid>
{
    public Guid TankId { get; set; }
    public string Name { get; set; }

    public virtual Tank Tank { get; set; }
    public virtual ICollection<Box> Boxes { get; set; }

    public Rack()
    {
        Boxes = new HashSet<Box>();
    }

    public int GetTotalSlotCount()
    {
        return Boxes?.Sum(b => b.Slots.Count) ?? 0;
    }

    public int GetOccupiedSlotCount()
    {
        return Boxes?.Sum(b => b.Slots.Count(s => s.IsOccupied)) ?? 0;
    }
}