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
                               .ThenInclude(r => r.Boxes)
                               .ThenInclude(b => b.Slots)
                               .ThenInclude(s => s.Bag),
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
                        Boxes = r.Boxes.OrderBy(b => b.Name).Select(b => new CryoBoxDto
                        {
                            Id = b.Id,
                            Name = b.Name,
                            Slots = b.Slots.OrderBy(s => s.Position).Select(s => new CryoSlotDto
                            {
                                Id = s.Id,
                                Position = s.Position,
                                IsOccupied = s.IsOccupied,
                                BagId = s.Bag?.Id,
                                BagNumber = s.Bag?.BagNumber,
                                Status = s.Bag != null ? s.Bag.Status.ToString() : null,
                                Purpose = s.Bag != null ? s.Bag.Purpose.ToString() : null,
                                Cd34PerKg = s.Bag?.Cd34PerKg,
                                Cd3PerKg = s.Bag?.Cd3PerKg,
                                LocationCode = $"{t.Name}-{r.Name}-{b.Name}-{s.Position}"
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
    public List<CryoBoxDto> Boxes { get; set; } = new();
}

public class CryoBoxDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<CryoSlotDto> Slots { get; set; } = new();
}

public class CryoSlotDto
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
