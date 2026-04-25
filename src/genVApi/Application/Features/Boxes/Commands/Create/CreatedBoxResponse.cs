using NArchitecture.Core.Application.Responses;

namespace Application.Features.Boxes.Commands.Create;

public class CreatedBoxResponse : IResponse
{
    public Guid Id { get; set; }
    public Guid SlotId { get; set; }
    public string Name { get; set; }
}