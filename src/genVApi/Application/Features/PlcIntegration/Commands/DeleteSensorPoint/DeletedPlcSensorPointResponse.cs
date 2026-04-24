using NArchitecture.Core.Application.Responses;

namespace Application.Features.PlcIntegration.Commands.DeleteSensorPoint;

public sealed class DeletedPlcSensorPointResponse : IResponse
{
    public Guid Id { get; set; }
}
