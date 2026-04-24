using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;
using NArchitecture.Core.Persistence.Repositories;
namespace Domain.Entities;
public class Tank : Entity<Guid>
{
    public string Name { get; set; }

    public virtual ICollection<Rack> Racks { get; set; }

    public Tank()
    {
        Racks = new HashSet<Rack>();
    }

    public int GetTotalSlotCount()
    {
        return Racks?.Sum(r => r.GetTotalSlotCount()) ?? 0;
    }

    public int GetOccupiedSlotCount()
    {
        return Racks?.Sum(r => r.GetOccupiedSlotCount()) ?? 0;
    }
}
