using Domain.Enums;
using NArchitecture.Core.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;
public class Bag : Entity<Guid>
{
    public Guid SessionId { get; set; }

    public int BagNumber { get; set; }

    public double VolumeMl { get; set; }
    public double SourceVolumeMl { get; set; }

    /// <summary>Torbanın (ürün bölümünün) kendi WBC sayımı. DMSO eklendikten sonra ürüne göre değişir.</summary>
    public double? Wbc { get; set; }

    /// <summary>
    /// Torba hesabında kullanılan flow cytometry yüzdeleri (session'dan kopyalanır, kayıt altına alınır).
    /// </summary>
    public double? Cd34Percent { get; set; }
    public double? Cd45Percent { get; set; }
    public double? Cd3Percent { get; set; }

    /// <summary>
    /// Ürünün torbaya bölünme notu (ör. "32+32=64 ml"). El yazısı kayıtlarla bire bir eşleşmesi için.
    /// </summary>
    public string? CompositionNote { get; set; }

    public double Cd34PerKg { get; set; }
    public double Cd3PerKg { get; set; }

    public BagStatus Status { get; set; }

    /// <summary>Torba &quot;Kullan&quot; aksiyonunda seçilen sebep (Infusion / Disposal / Transfer / Other).</summary>
    public BagUseReason? UseReason { get; set; }

    /// <summary>&quot;Kullan&quot; sırasında girilen serbest not. UseReason == Other ise zorunludur.</summary>
    public string? UseNote { get; set; }

    /// <summary>Torbanın amacı (Cryo, Infusion, Backup, QC).</summary>
    public BagPurpose Purpose { get; set; } = BagPurpose.Cryo;

    /// <summary>
    /// Aynı aferez ürününden birlikte bölünen torbaları gruplamak için kullanılır.
    /// Aynı gruptaki 4 torba aynı SplitBatchId'yi taşır.
    /// </summary>
    public Guid? SplitBatchId { get; set; }

    public Guid? BagCellId { get; set; }

    public virtual CollectionSession Session { get; set; }
    public virtual BagCell? BagCell { get; set; }

    public bool IsOptimal()
        => Cd34PerKg >= 4 && Cd3PerKg >= 3 && Cd3PerKg <= 8;

    public bool IsHighRisk()
        => Cd3PerKg > 10;

    public bool IsLowImmunity()
        => Cd3PerKg < 2;

    public bool IsUsable()
        => Status == BagStatus.Stored;

    public void MarkAsUsed()
    {
        if (Status == BagStatus.Used)
            throw new Exception("Already used");

        Status = BagStatus.Used;
    }
}