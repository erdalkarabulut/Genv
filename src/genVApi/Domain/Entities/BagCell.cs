using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;

/// <summary>
/// Kutu içi torba konumu (grid hücresi). Hiyerarşi: Tank → Rack → Slot → Box → BagCell.
/// </summary>
public class BagCell : Entity<Guid>
{
    public Guid BoxId { get; set; }
    public string Position { get; set; } = default!;

    public bool IsOccupied { get; set; }
    public int Version { get; set; }

    public virtual Box Box { get; set; } = default!;
    public virtual Bag? Bag { get; set; }

    public bool CanAssign() => !IsOccupied;

    public void AssignBag(Bag bag)
    {
        if (IsOccupied)
            throw new Exception("BagCell already occupied");

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
        return $"{Box?.Slot?.Rack?.Tank?.Name}-{Box?.Slot?.Rack?.Name}-{Box?.Slot?.Name}-{Box?.Name}-{Position}";
    }
}
