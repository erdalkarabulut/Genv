using NArchitecture.Core.Application.Responses;

namespace Application.Features.Tanks.Queries.GetById;

public class GetByIdTankResponse : IResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}