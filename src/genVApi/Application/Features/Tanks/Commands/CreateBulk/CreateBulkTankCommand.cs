using Application.Features.Tanks.Constants;
using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Tanks.Constants.TanksOperationClaims;

namespace Application.Features.Tanks.Commands.CreateBulk;

public class CreateBulkTankCommand : IRequest<CreateBulkTankResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    /// <summary>
    /// Mevcut bir tanka ekleme yapılacaksa tank ID'si, yeni tank oluşturulacaksa null
    /// </summary>
    public Guid? ExistingTankId { get; set; }

    public string TankName { get; set; }
    public int RackCount { get; set; }
    public int SlotsPerRack { get; set; }
    public int BoxesPerSlot { get; set; }
    public int CellsPerBox { get; set; }

    /// <summary>Rack isimleri için önek (boş bırakılırsa R, Raf, Shelf vb.)</summary>
    public string RackPrefix { get; set; } = "";
    /// <summary>Slot isimleri için önek (boş bırakılırsa S, Slot vb.)</summary>
    public string SlotPrefix { get; set; } = "";
    /// <summary>Box isimleri için önek (boş bırakılırsa B, Kutu vb.)</summary>
    public string BoxPrefix { get; set; } = "";
    /// <summary>Torba hücresi pozisyonları için harf öneki (boş bırakılırsa sadece sayı)</summary>
    public string CellPrefix { get; set; } = "";

    public string[] Roles => [Admin, Write, TanksOperationClaims.Create];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetTanks", "GetCryoGrid"];

    public class CreateBulkTankCommandHandler : IRequestHandler<CreateBulkTankCommand, CreateBulkTankResponse>
    {
        private readonly ITankRepository _tankRepository;
        private readonly IRackRepository _rackRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly IBoxRepository _boxRepository;
        private readonly IBagCellRepository _bagCellRepository;

        public CreateBulkTankCommandHandler(
            ITankRepository tankRepository,
            IRackRepository rackRepository,
            ISlotRepository slotRepository,
            IBoxRepository boxRepository,
            IBagCellRepository bagCellRepository)
        {
            _tankRepository = tankRepository;
            _rackRepository = rackRepository;
            _slotRepository = slotRepository;
            _boxRepository = boxRepository;
            _bagCellRepository = bagCellRepository;
        }

        public async Task<CreateBulkTankResponse> Handle(CreateBulkTankCommand request, CancellationToken cancellationToken)
        {
            Tank tank;

            if (request.ExistingTankId.HasValue)
            {
                tank = await _tankRepository.GetAsync(t => t.Id == request.ExistingTankId.Value)
                    ?? throw new Exception("Tank bulunamadı.");
            }
            else
            {
                tank = new Tank
                {
                    Id = Guid.NewGuid(),
                    Name = request.TankName
                };
                await _tankRepository.AddAsync(tank);
            }

            var createdRacks = 0;
            var createdSlots = 0;
            var createdBoxes = 0;
            var createdCells = 0;

            // Mevcut rack sayısını bul (yeni rack'leri ondan sonra numaralandırmak için)
            var existingRackCount = tank.Racks?.Count ?? 0;

            for (int r = 1; r <= request.RackCount; r++)
            {
                var rack = new Rack
                {
                    Id = Guid.NewGuid(),
                    TankId = tank.Id,
                    Name = $"{request.RackPrefix}{existingRackCount + r:000}"
                };
                await _rackRepository.AddAsync(rack);
                createdRacks++;

                for (int s = 1; s <= request.SlotsPerRack; s++)
                {
                    var slot = new Slot
                    {
                        Id = Guid.NewGuid(),
                        RackId = rack.Id,
                        Name = $"{request.SlotPrefix}{s}"
                    };
                    await _slotRepository.AddAsync(slot);
                    createdSlots++;

                    for (int b = 1; b <= request.BoxesPerSlot; b++)
                    {
                        var box = new Box
                        {
                            Id = Guid.NewGuid(),
                            SlotId = slot.Id,
                            Name = $"{request.BoxPrefix}{b}"
                        };
                        await _boxRepository.AddAsync(box);
                        createdBoxes++;

                        for (int c = 1; c <= request.CellsPerBox; c++)
                        {
                            var cell = new BagCell
                            {
                                Id = Guid.NewGuid(),
                                BoxId = box.Id,
                                Position = string.IsNullOrWhiteSpace(request.CellPrefix)
                                    ? c.ToString()
                                    : $"{request.CellPrefix}{c}",
                                IsOccupied = false
                            };
                            await _bagCellRepository.AddAsync(cell);
                            createdCells++;
                        }
                    }
                }
            }

            return new CreateBulkTankResponse
            {
                TankId = tank.Id,
                TankName = tank.Name,
                TotalRacks = createdRacks,
                TotalSlots = createdSlots,
                TotalBoxes = createdBoxes,
                TotalCells = createdCells
            };
        }
    }
}
