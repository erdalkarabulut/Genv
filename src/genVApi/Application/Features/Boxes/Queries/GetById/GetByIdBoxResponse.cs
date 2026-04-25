using NArchitecture.Core.Application.Responses;

namespace Application.Features.Boxes.Queries.GetById;

public class GetByIdBoxResponse : IResponse
{
    public Guid Id { get; set; }
    public Guid SlotId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}