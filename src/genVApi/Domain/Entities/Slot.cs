using NArchitecture.Core.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;
public class Slot : Entity<Guid>
{
    public Guid BoxId { get; set; }
    public string Position { get; set; }

    public bool IsOccupied { get; set; }
    public int Version { get; set; }

    public virtual Box Box { get; set; }
    public virtual Bag? Bag { get; set; }

    public bool CanAssign() => !IsOccupied;

    public void AssignBag(Bag bag)
    {
        if (IsOccupied)
            throw new Exception("Slot already occupied");

        Bag = bag;
        IsOccupied = true;
    }

    public void RemoveBag()
    {
        Bag = null;
        IsOccupied = false;
    }

    public string GetFullLocation()
    {
        return $"{Box?.Rack?.Tank?.Name}-{Box?.Rack?.Name}-{Box?.Name}-{Position}";
    }
}