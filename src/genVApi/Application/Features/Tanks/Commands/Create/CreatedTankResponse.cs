using NArchitecture.Core.Application.Responses;

namespace Application.Features.Tanks.Commands.Create;

public class CreatedTankResponse : IResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}