using NArchitecture.Core.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;
public class BagMovement : Entity<Guid>
{
    public Guid BagId { get; set; }
    public Guid? FromSlotId { get; set; }
    public Guid? ToSlotId { get; set; }

    public string Action { get; set; }

    public virtual Bag Bag { get; set; }

    public string GetSummary()
    {
        return $"{Action} - {CreatedDate}";
    }
}
