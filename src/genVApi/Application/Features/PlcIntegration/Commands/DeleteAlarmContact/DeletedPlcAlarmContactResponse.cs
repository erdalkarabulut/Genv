using NArchitecture.Core.Application.Responses;

namespace Application.Features.PlcIntegration.Commands.DeleteAlarmContact;

public sealed class DeletedPlcAlarmContactResponse : IResponse
{
    public Guid Id { get; set; }
}
