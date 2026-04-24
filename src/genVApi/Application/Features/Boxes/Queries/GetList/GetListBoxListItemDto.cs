using NArchitecture.Core.Application.Dtos;

namespace Application.Features.Boxes.Queries.GetList;

public class GetListBoxListItemDto : IDto
{
    public Guid Id { get; set; }
    public Guid RackId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }
}