using NArchitecture.Core.Application.Responses;

namespace Application.Features.Tanks.Commands.Update;

public class UpdatedTankResponse : IResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}