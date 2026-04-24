using NArchitecture.Core.Application.Responses;

namespace Application.Features.Racks.Commands.Update;

public class UpdatedRackResponse : IResponse
{
    public Guid Id { get; set; }
    public Guid TankId { get; set; }
    public string Name { get; set; }
}