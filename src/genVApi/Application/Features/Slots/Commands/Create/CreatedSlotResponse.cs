using NArchitecture.Core.Application.Responses;

namespace Application.Features.Slots.Commands.Create;

public class CreatedSlotResponse : IResponse
{
    public Guid Id { get; set; }
    public Guid BoxId { get; set; }
    public string Position { get; set; }
    public bool IsOccupied { get; set; }
    public int Version { get; set; }
}