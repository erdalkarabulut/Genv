namespace Domain.Enums;

/// <summary>
/// Bag &quot;Kullan&quot; aksiyonu sebebi. Other seçildiğinde UseNote zorunludur.
/// </summary>
public enum BagUseReason
{
    Infusion = 1,
    Disposal = 2,
    Transfer = 3,
    Other = 4
}
