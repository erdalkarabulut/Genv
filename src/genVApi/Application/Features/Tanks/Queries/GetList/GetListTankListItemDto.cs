using NArchitecture.Core.Application.Dtos;

namespace Application.Features.Tanks.Queries.GetList;

public class GetListTankListItemDto : IDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }
}