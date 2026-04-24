using NArchitecture.Core.Application.Dtos;

namespace Application.Features.OtpAuthenticators.Queries.GetList;

public class GetListOtpAuthenticatorListItemDto : IDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string SecretKey { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
}
