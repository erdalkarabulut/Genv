using NArchitecture.Core.Application.Responses;

namespace Application.Features.Boxes.Commands.Update;

public class UpdatedBoxResponse : IResponse
{
    public Guid Id { get; set; }
    public Guid RackId { get; set; }
    public string Name { get; set; }
}