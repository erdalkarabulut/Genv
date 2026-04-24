using NArchitecture.Core.Security.Attributes;

namespace Application.Features.Bags.Constants;

[OperationClaimConstants]
public static class BagsOperationClaims
{
    private const string _section = "Bags";

    public const string Admin = $"{_section}.Admin";

    public const string Read = $"{_section}.Read";
    public const string Write = $"{_section}.Write";

    public const string Create = $"{_section}.Create";
    public const string Update = $"{_section}.Update";
    public const string Delete = $"{_section}.Delete";

    public const string CreateRange = $"{_section}.CreateRange";
    public const string UpdateRange = $"{_section}.UpdateRange";
    public const string DeleteRange = $"{_section}.DeleteRange";
}