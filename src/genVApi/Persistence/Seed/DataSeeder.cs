using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NArchitecture.Core.Security.Enums;
using NArchitecture.Core.Security.Hashing;
using Persistence.Contexts;

namespace Persistence.Seed;

/// <summary>
/// Demo seed data — tüm klinik senaryo ve operasyon durumlarını kapsar.
///
/// Senaryolar:
///   P1 · Otolog · 1 gün · Optimal (CD34 ≥ 4, CD3 3–8) · 4 torba + Cryo store
///   P2 · Otolog · 2 gün · Sınırda (≥ target, &lt; ideal) · henüz split yok → demoda bölünebilir
///   P3 · Otolog · 4 gün · Yetersiz · Max days reached — "klinik değerlendirme gerekir"
///   P4 · Allogeneik · 1 gün · GVHD riski (CD3 &gt; 10) · 4 torba + Cryo store
///   P5 · Allogeneik · 2 gün · Optimal · 4 torba + Cryo store + bir torba taşındı + infüzyon kullanıldı
///   P6 · Allogeneik · 2 gün · Düşük bağışıklık (CD3 &lt; 2) · henüz split yok
/// </summary>
public static class DataSeeder
{
    public static async Task SeedDataAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<BaseDbContext>();
        await SeedUsersAsync(ctx);
        await SeedAsync(ctx);
    }

    /// <summary>
    /// Default demo admin: admin@genvapi.com / Admin123!
    /// </summary>
    private static async Task SeedUsersAsync(BaseDbContext ctx)
    {
        const string email = "admin@genvapi.com";
        if (await ctx.Users.AnyAsync(u => u.Email == email))
            return;

        HashingHelper.CreatePasswordHash("Admin123!", out byte[] hash, out byte[] salt);

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            AuthenticatorType = AuthenticatorType.None
        };
        ctx.Users.Add(admin);

        // Bind to the root "Admin" operation claim (seeded by OperationClaimConfiguration, Id = 1).
        ctx.UserOperationClaims.Add(new UserOperationClaim
        {
            Id = Guid.NewGuid(),
            UserId = admin.Id,
            OperationClaimId = 1
        });

        await ctx.SaveChangesAsync();
    }

    private static async Task SeedAsync(BaseDbContext ctx)
    {
        if (await ctx.Tanks.AnyAsync())
        {
            await EnsureLargeCryoDemoTankAsync(ctx);
            return;
        }

        var now = DateTime.UtcNow;

        // =========================================================
        // 1) Tank → Rack → Slot (raf) → Box → BagCell (3×3 = 9 hücre/kutu)
        //    TankA: 2 rack × 2 raf-slot × 9 bagcell = 36 hücre
        //    TankB: 1 rack × 1 raf-slot × 9 bagcell = 9 hücre
        // =========================================================
        CreateTank(ctx, "TankA", racks: new[] { "R1", "R2" }, boxesPerRack: new[] { "B1", "B2" });
        CreateTank(ctx, "TankB", racks: new[] { "R1" }, boxesPerRack: new[] { "B1" });

        // Torba hücrelerini pozisyona göre kuyruk — demo torbalar sırayla yerleşsin.
        var bagCellQueue = new Queue<BagCell>(
            ctx.ChangeTracker.Entries<BagCell>()
                .Select(e => e.Entity)
                .OrderBy(c => c.BoxId)
                .ThenBy(c => c.Position));

        // =========================================================
        // 2) Donors (farklı yakınlık dereceleri)
        // =========================================================
        var donorSibling = new Donor
        {
            Id = Guid.NewGuid(),
            FullName = "Murat Kara (Kardeş)",
            WeightKg = 72,
            BloodGroup = "A+",
            Relation = "Sibling"
        };
        var donorParent = new Donor
        {
            Id = Guid.NewGuid(),
            FullName = "Serap Tunç (Anne)",
            WeightKg = 66,
            BloodGroup = "0-",
            Relation = "Parent"
        };
        var donorUnrelated = new Donor
        {
            Id = Guid.NewGuid(),
            FullName = "MUD Donor 2026-17",
            WeightKg = 70,
            BloodGroup = "B+",
            Relation = "MUD"
        };
        ctx.Donors.AddRange(donorSibling, donorParent, donorUnrelated);

        // =========================================================
        // 3) P1 · Otolog · Gün 1'de ideal → split + cryo store
        //     Sonuç: Optimal, 4 torba (1 Stored, 3 Reserved)
        // =========================================================
        var p1 = AddPatient(ctx,
            name: "John Doe",
            kg: 70, blood: "A+",
            type: TransplantType.Autologous,
            diagnosis: "Multiple Myeloma",
            protocol: "PR-2026-001",
            birthYear: 1965);

        var p1s1 = AddSession(ctx, p1.Id, day: 1, date: now.AddDays(-6),
            wbcPre: 14, hgb: 11.8, hct: 34, plt: 180,
            volumeMl: 260, wbc: 220,
            cd34Pct: 1.6, cd45Pct: 82, cd3Pct: 25, lymphoPct: 30, mhs: 1.05,
            cd34PerKg: 5.8, cd3PerKg: 5.1);
        SplitIntoBags(ctx, p1s1, bagCellQueue, moveCryoToNewSlot: false, markInfusionUsed: false);

        // =========================================================
        // 4) P2 · Otolog · 2 gün sınırda (≥ target, &lt; ideal)
        //     Kümülatif CD34 ≈ 3.3 → Sınırda. Torba bölme bilerek yapılmadı;
        //     demoda "4 torbaya böl + Cryo" akışı burada denenebilir.
        // =========================================================
        var p2 = AddPatient(ctx,
            name: "Fatma Kara",
            kg: 65, blood: "B+",
            type: TransplantType.Autologous,
            diagnosis: "Non-Hodgkin Lymphoma",
            protocol: "PR-2026-002",
            birthYear: 1972);

        AddSession(ctx, p2.Id, day: 1, date: now.AddDays(-3),
            wbcPre: 9, hgb: 10.2, hct: 31, plt: 150,
            volumeMl: 230, wbc: 160,
            cd34Pct: 0.8, cd45Pct: 79, cd3Pct: 18, lymphoPct: 25, mhs: 0.8,
            cd34PerKg: 1.8, cd3PerKg: 2.3);
        AddSession(ctx, p2.Id, day: 2, date: now.AddDays(-2),
            wbcPre: 8, hgb: 10.0, hct: 30, plt: 140,
            volumeMl: 240, wbc: 150,
            cd34Pct: 0.7, cd45Pct: 78, cd3Pct: 16, lymphoPct: 23, mhs: 0.75,
            cd34PerKg: 1.5, cd3PerKg: 2.1);

        // =========================================================
        // 5) P3 · Otolog · 4 gün hedefe ulaşılamadı
        //     Max days reached + Yetersiz → "klinik değerlendirme gerekir"
        // =========================================================
        var p3 = AddPatient(ctx,
            name: "Ali Veli",
            kg: 80, blood: "0+",
            type: TransplantType.Autologous,
            diagnosis: "AML (induction failure)",
            protocol: "PR-2026-003",
            birthYear: 1968);

        for (int d = 1; d <= 4; d++)
        {
            AddSession(ctx, p3.Id, day: d, date: now.AddDays(-8 + d),
                wbcPre: 6, hgb: 9.5, hct: 28, plt: 130,
                volumeMl: 220, wbc: 100,
                cd34Pct: 0.3, cd45Pct: 70, cd3Pct: 10, lymphoPct: 15, mhs: 0.4,
                cd34PerKg: 0.4, cd3PerKg: 0.8);
        }
        // Kümülatif CD34 ≈ 1.6 (< target 2) · 4. gün dolmuş → max reached

        // =========================================================
        // 6) P4 · Allogeneik · Gün 1 + split
        //     CD34 ≈ 6.2 (optimal) fakat CD3 ≈ 12.5 → GVHD riski
        // =========================================================
        var p4 = AddPatient(ctx,
            name: "Mehmet Solmaz",
            kg: 68, blood: "A+",
            type: TransplantType.Allogeneic,
            diagnosis: "CML (blast phase)",
            protocol: "PR-2026-004",
            birthYear: 1970,
            donorId: donorSibling.Id);

        var p4s1 = AddSession(ctx, p4.Id, day: 1, date: now.AddDays(-2),
            wbcPre: 20, hgb: 12.5, hct: 36, plt: 200,
            volumeMl: 280, wbc: 250,
            cd34Pct: 1.8, cd45Pct: 85, cd3Pct: 40, lymphoPct: 55, mhs: 1.2,
            cd34PerKg: 6.2, cd3PerKg: 12.5);
        SplitIntoBags(ctx, p4s1, bagCellQueue, moveCryoToNewSlot: false, markInfusionUsed: false);

        // =========================================================
        // 7) P5 · Allogeneik · 2 gün optimal + split + move + use
        //     Kümülatif CD34 ≈ 5.1, CD3 ≈ 5.3 → Optimal
        //     Cryo torbası bir kez taşındı, Infusion torbası kullanıldı (geçmiş operasyon).
        // =========================================================
        var p5 = AddPatient(ctx,
            name: "Ayşe Tunç",
            kg: 60, blood: "0-",
            type: TransplantType.Allogeneic,
            diagnosis: "AML (MRD positive)",
            protocol: "PR-2026-005",
            birthYear: 1980,
            donorId: donorParent.Id);

        AddSession(ctx, p5.Id, day: 1, date: now.AddDays(-6),
            wbcPre: 12, hgb: 11.0, hct: 33, plt: 160,
            volumeMl: 250, wbc: 180,
            cd34Pct: 0.9, cd45Pct: 80, cd3Pct: 20, lymphoPct: 28, mhs: 0.95,
            cd34PerKg: 2.5, cd3PerKg: 2.6);
        var p5s2 = AddSession(ctx, p5.Id, day: 2, date: now.AddDays(-5),
            wbcPre: 13, hgb: 11.2, hct: 34, plt: 170,
            volumeMl: 260, wbc: 200,
            cd34Pct: 1.1, cd45Pct: 80, cd3Pct: 22, lymphoPct: 30, mhs: 1.0,
            cd34PerKg: 2.6, cd3PerKg: 2.7);
        SplitIntoBags(ctx, p5s2, bagCellQueue, moveCryoToNewSlot: true, markInfusionUsed: true);

        // =========================================================
        // 8) P6 · Allogeneik · 2 gün · Düşük bağışıklık
        //     Kümülatif CD34 ≈ 5.1 · CD3 ≈ 1.6 → Düşük bağışıklık uyarısı
        //     Split henüz yapılmadı.
        // =========================================================
        var p6 = AddPatient(ctx,
            name: "Hakan Dalgıç",
            kg: 75, blood: "AB+",
            type: TransplantType.Allogeneic,
            diagnosis: "ALL (Ph+)",
            protocol: "PR-2026-006",
            birthYear: 1988,
            donorId: donorUnrelated.Id);

        AddSession(ctx, p6.Id, day: 1, date: now.AddDays(-4),
            wbcPre: 11, hgb: 10.8, hct: 32, plt: 155,
            volumeMl: 240, wbc: 190,
            cd34Pct: 1.2, cd45Pct: 80, cd3Pct: 6, lymphoPct: 15, mhs: 1.0,
            cd34PerKg: 2.5, cd3PerKg: 0.7);
        AddSession(ctx, p6.Id, day: 2, date: now.AddDays(-3),
            wbcPre: 11, hgb: 10.5, hct: 31, plt: 150,
            volumeMl: 250, wbc: 200,
            cd34Pct: 1.1, cd45Pct: 80, cd3Pct: 5, lymphoPct: 14, mhs: 1.0,
            cd34PerKg: 2.6, cd3PerKg: 0.9);

        await ctx.SaveChangesAsync();

        await EnsureLargeCryoDemoTankAsync(ctx);
    }

    // -----------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------

    private static void CreateTank(BaseDbContext ctx, string name, string[] racks, string[] boxesPerRack)
    {
        var tank = new Tank { Id = Guid.NewGuid(), Name = name };
        ctx.Tanks.Add(tank);

        foreach (var rackName in racks)
        {
            var rack = new Rack { Id = Guid.NewGuid(), TankId = tank.Id, Name = rackName };
            ctx.Racks.Add(rack);

            foreach (var boxName in boxesPerRack)
            {
                var rackSlot = new Slot { Id = Guid.NewGuid(), RackId = rack.Id, Name = boxName };
                ctx.RackSlots.Add(rackSlot);

                var box = new Box { Id = Guid.NewGuid(), SlotId = rackSlot.Id, Name = boxName };
                ctx.Boxes.Add(box);

                // 3×3 = 9 torba hücresi: A1..A3, B1..B3, C1..C3
                foreach (var row in new[] { 'A', 'B', 'C' })
                {
                    for (int col = 1; col <= 3; col++)
                    {
                        ctx.BagCells.Add(new BagCell
                        {
                            Id = Guid.NewGuid(),
                            BoxId = box.Id,
                            Position = $"{row}{col}",
                            IsOccupied = false,
                            Version = 0
                        });
                    }
                }
            }
        }
    }

    private static Patient AddPatient(BaseDbContext ctx,
        string name, double kg, string blood,
        TransplantType type, string diagnosis, string protocol,
        int birthYear, Guid? donorId = null)
    {
        var p = new Patient
        {
            Id = Guid.NewGuid(),
            FullName = name,
            WeightKg = kg,
            BloodGroup = blood,
            TransplantType = type,
            Diagnosis = diagnosis,
            ProtocolNo = protocol,
            BirthDate = new DateTime(birthYear, 3, 15).ToUniversalTime(),
            DonorId = donorId
        };
        ctx.Patients.Add(p);
        return p;
    }

    private static CollectionSession AddSession(BaseDbContext ctx,
        Guid patientId, int day, DateTime date,
        double wbcPre, double hgb, double hct, double plt,
        double volumeMl, double wbc,
        double cd34Pct, double cd45Pct, double cd3Pct, double lymphoPct, double mhs,
        double cd34PerKg, double cd3PerKg)
    {
        var s = new CollectionSession
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            Day = day,
            Date = date,
            WbcPre = wbcPre,
            Hgb = hgb,
            Hct = hct,
            Plt = plt,
            VolumeMl = volumeMl,
            WBC = wbc,
            Cd34Percent = cd34Pct,
            Cd45Percent = cd45Pct,
            Cd3Percent = cd3Pct,
            LymphocytePercent = lymphoPct,
            Mhs = mhs,
            Cd34PerKg = cd34PerKg,
            Cd3PerKg = cd3PerKg
        };
        ctx.CollectionSessions.Add(s);
        return s;
    }

    /// <summary>
    /// Aferez ürününü 4 torbaya böler, Cryo torbasını kuyruktan ilk boş slota store eder
    /// ve gerekirse bir kez taşıma (Move) ve bir Infusion torbası kullanma (Use) akışını da loglar.
    /// </summary>
    private static void SplitIntoBags(BaseDbContext ctx, CollectionSession session, Queue<BagCell> bagCellQueue,
        bool moveCryoToNewSlot, bool markInfusionUsed)
    {
        var splitBatchId = Guid.NewGuid();
        var perVolume = Math.Round(session.VolumeMl / 4, 2);
        var perCd34 = Math.Round(session.Cd34PerKg / 4, 4);
        var perCd3 = Math.Round(session.Cd3PerKg / 4, 4);

        var purposes = new[]
        {
            BagPurpose.Cryo,
            BagPurpose.Infusion,
            BagPurpose.Backup,
            BagPurpose.QualityControl
        };

        var bags = new List<Bag>();
        for (int i = 0; i < 4; i++)
        {
            var bag = new Bag
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                BagNumber = i + 1,
                VolumeMl = perVolume,
                SourceVolumeMl = session.VolumeMl,
                Cd34PerKg = perCd34,
                Cd3PerKg = perCd3,
                Purpose = purposes[i],
                SplitBatchId = splitBatchId,
                Status = purposes[i] == BagPurpose.Cryo
                    ? BagStatus.Frozen
                    : BagStatus.Reserved
            };
            ctx.Bags.Add(bag);
            bags.Add(bag);
        }

        // Cryo → store to next empty bag cell
        var cryo = bags.First(b => b.Purpose == BagPurpose.Cryo);
        if (bagCellQueue.Count > 0)
        {
            var cell = bagCellQueue.Dequeue();
            cell.IsOccupied = true;
            cell.Version += 1;

            cryo.BagCellId = cell.Id;
            cryo.Status = BagStatus.Stored;

            ctx.BagMovements.Add(new BagMovement
            {
                Id = Guid.NewGuid(),
                BagId = cryo.Id,
                FromBagCellId = null,
                ToBagCellId = cell.Id,
                Action = "Split-Store (Cryo)"
            });

            if (moveCryoToNewSlot && bagCellQueue.Count > 0)
            {
                var newCell = bagCellQueue.Dequeue();

                cell.IsOccupied = false;
                cell.Version += 1;

                newCell.IsOccupied = true;
                newCell.Version += 1;

                cryo.BagCellId = newCell.Id;

                ctx.BagMovements.Add(new BagMovement
                {
                    Id = Guid.NewGuid(),
                    BagId = cryo.Id,
                    FromBagCellId = cell.Id,
                    ToBagCellId = newCell.Id,
                    Action = "Move"
                });
            }
        }

        if (markInfusionUsed)
        {
            var infusion = bags.First(b => b.Purpose == BagPurpose.Infusion);
            infusion.Status = BagStatus.Used;

            ctx.BagMovements.Add(new BagMovement
            {
                Id = Guid.NewGuid(),
                BagId = infusion.Id,
                FromBagCellId = null,
                ToBagCellId = null,
                Action = "Use"
            });
        }
    }

    private static async Task EnsureLargeCryoDemoTankAsync(BaseDbContext ctx)
    {
        const string largeTankName = "Tank-XL-200";
        bool exists = await ctx.Tanks.AnyAsync(t => t.Name == largeTankName);
        if (exists)
            return;

        var tank = new Tank { Id = Guid.NewGuid(), Name = largeTankName };
        ctx.Tanks.Add(tank);

        for (int rackNo = 1; rackNo <= 200; rackNo++)
        {
            var rack = new Rack
            {
                Id = Guid.NewGuid(),
                TankId = tank.Id,
                Name = $"R{rackNo:000}"
            };
            ctx.Racks.Add(rack);

            var slot = new Slot
            {
                Id = Guid.NewGuid(),
                RackId = rack.Id,
                Name = "S1"
            };
            ctx.RackSlots.Add(slot);

            var box = new Box
            {
                Id = Guid.NewGuid(),
                SlotId = slot.Id,
                Name = "B1"
            };
            ctx.Boxes.Add(box);

            for (int i = 1; i <= 4; i++)
            {
                ctx.BagCells.Add(new BagCell
                {
                    Id = Guid.NewGuid(),
                    BoxId = box.Id,
                    Position = $"A{i}",
                    IsOccupied = false,
                    Version = 0
                });
            }
        }

        await ctx.SaveChangesAsync();
    }
}
