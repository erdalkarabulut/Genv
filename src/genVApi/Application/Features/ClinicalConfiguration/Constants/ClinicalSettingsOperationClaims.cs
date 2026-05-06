namespace Application.Features.ClinicalConfiguration.Constants;

public static class ClinicalSettingsOperationClaims
{
    private const string _section = "ClinicalSettings";

    public const string Admin = $"{_section}.Admin";
    public const string Read = $"{_section}.Read";
    public const string Write = $"{_section}.Write";
}
