using NArchitecture.Core.Application.Responses;

namespace Application.Features.PlcIntegration.Commands.UpdateAlarmContact;

public sealed class UpdatedPlcAlarmContactResponse : IResponse
{
    public Guid Id { get; set; }
}
