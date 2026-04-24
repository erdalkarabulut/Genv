using NArchitecture.Core.Application.Responses;

namespace Application.Features.PlcIntegration.Commands.UpdateSensorPoint;

public sealed class UpdatedPlcSensorPointResponse : IResponse
{
    public Guid Id { get; set; }
}
