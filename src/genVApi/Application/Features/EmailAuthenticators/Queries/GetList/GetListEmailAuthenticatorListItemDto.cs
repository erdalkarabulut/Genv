using NArchitecture.Core.Application.Dtos;

namespace Application.Features.EmailAuthenticators.Queries.GetList;

public class GetListEmailAuthenticatorListItemDto : IDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? ActivationKey { get; set; }
    public bool IsVerified { get; set; }
}
