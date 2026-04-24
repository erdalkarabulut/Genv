using NArchitecture.Core.Application.Responses;

namespace Application.Features.Racks.Queries.GetById;

public class GetByIdRackResponse : IResponse
{
    public Guid Id { get; set; }
    public Guid TankId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}