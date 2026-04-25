using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Slots.Queries.GetCryoGrid;

public class GetCryoGridQuery : IRequest<CryoGridResponse>
{
    public Guid? TankId { get; set; }

    public class GetCryoGridQueryHandler : IRequestHandler<GetCryoGridQuery, CryoGridResponse>
    {
        private readonly ITankRepository _tankRepository;

        public GetCryoGridQueryHandler(ITankRepository tankRepository)
        {
            _tankRepository = tankRepository;
        }

        public async Task<CryoGridResponse> Handle(GetCryoGridQuery request, CancellationToken cancellationToken)
        {
            var query = await _tankRepository.GetListAsync(
                predicate: request.TankId.HasValue ? (t => t.Id == request.TankId.Value) : null,
                include: q => q.Include(t => t.Racks)
                    .ThenInclude(r => r.Slots)
                    .ThenInclude(s => s.Boxes)
                    .ThenInclude(b => b.BagCells)
                    .ThenInclude(c => c.Bag!),
                size: int.MaxValue,
                enableTracking: false,
                cancellationToken: cancellationToken
            );

            var response = new CryoGridResponse
            {
                Tanks = query.Items.Select(t => new CryoTankDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Racks = t.Racks.OrderBy(r => r.Name).Select(r => new CryoRackDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Slots = r.Slots.OrderBy(s => s.Name).Select(s => new CryoRackSlotDto
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Boxes = s.Boxes.OrderBy(b => b.Name).Select(b => new CryoBoxDto
                            {
                                Id = b.Id,
                                Name = b.Name,
                                BagCells = b.BagCells.OrderBy(c => c.Position).Select(c => new CryoBagCellDto
                                {
                                    Id = c.Id,
                                    Position = c.Position,
                                    IsOccupied = c.IsOccupied,
                                    BagId = c.Bag?.Id,
                                    BagNumber = c.Bag?.BagNumber,
                                    Status = c.Bag != null ? c.Bag.Status.ToString() : null,
                                    Purpose = c.Bag != null ? c.Bag.Purpose.ToString() : null,
                                    Cd34PerKg = c.Bag?.Cd34PerKg,
                                    Cd3PerKg = c.Bag?.Cd3PerKg,
                                    LocationCode =
                                        $"{t.Name}-{r.Name}-{s.Name}-{b.Name}-{c.Position}"
                                }).ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            return response;
        }
    }
}

public class CryoGridResponse
{
    public List<CryoTankDto> Tanks { get; set; } = new();
}

public class CryoTankDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<CryoRackDto> Racks { get; set; } = new();
}

public class CryoRackDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    /// <summary>Raf slotları (Tank → Rack → Slot).</summary>
    public List<CryoRackSlotDto> Slots { get; set; } = new();
}

public class CryoRackSlotDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<CryoBoxDto> Boxes { get; set; } = new();
}

public class CryoBoxDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<CryoBagCellDto> BagCells { get; set; } = new();
}

public class CryoBagCellDto
{
    public Guid Id { get; set; }
    public string Position { get; set; } = default!;
    public bool IsOccupied { get; set; }
    public Guid? BagId { get; set; }
    public int? BagNumber { get; set; }
    public string? Status { get; set; }
    public string? Purpose { get; set; }
    public double? Cd34PerKg { get; set; }
    public double? Cd3PerKg { get; set; }
    public string LocationCode { get; set; } = default!;
}
