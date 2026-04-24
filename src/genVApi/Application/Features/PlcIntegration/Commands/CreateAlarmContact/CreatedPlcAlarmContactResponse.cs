using NArchitecture.Core.Application.Responses;

namespace Application.Features.PlcIntegration.Commands.CreateAlarmContact;

public sealed class CreatedPlcAlarmContactResponse : IResponse
{
    public Guid Id { get; set; }
}
