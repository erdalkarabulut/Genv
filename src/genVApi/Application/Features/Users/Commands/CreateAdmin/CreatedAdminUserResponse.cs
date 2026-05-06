using NArchitecture.Core.Application.Responses;

namespace Application.Features.Users.Commands.CreateAdmin;

public class CreatedAdminUserResponse : IResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public bool Status { get; set; }
    public List<string> OperationClaimNames { get; set; } = new();

    public CreatedAdminUserResponse()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
    }
}
