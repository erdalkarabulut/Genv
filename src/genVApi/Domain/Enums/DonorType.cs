namespace Domain.Enums;

/// <summary>
/// Allojenik aferezde donörün hasta ile ilişkisi.
/// Formdaki "Akraba / Akraba-dışı" seçimine karşılık gelir.
/// </summary>
public enum DonorType
{
    /// <summary>Akraba donör (kardeş, ebeveyn vb.).</summary>
    Related = 0,
    /// <summary>Akraba-dışı donör (unrelated; ulusal/uluslararası kaynaklı).</summary>
    Unrelated = 1
}
