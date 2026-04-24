namespace Domain.Enums;

/// <summary>
/// Torba amacı. Aferez ürünü 4 torbaya bölündüğünde her torbaya bir amaç atanır.
/// Varsayılan dağılım: 1 Cryo (tanka), 1 Infusion (infüzyon), 1 Backup (yedek), 1 QualityControl (QC / numune).
/// </summary>
public enum BagPurpose
{
    Cryo = 0,
    Infusion = 1,
    Backup = 2,
    QualityControl = 3
}
