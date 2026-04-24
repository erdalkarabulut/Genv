using NArchitecture.Core.Application.Responses;

namespace Application.Features.Slots.Queries.GetById;

public class GetByIdSlotResponse : IResponse
{
    public Guid Id { get; set; }
    public Guid BoxId { get; set; }
    public string Position { get; set; }
    public bool IsOccupied { get; set; }
    public int Version { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}