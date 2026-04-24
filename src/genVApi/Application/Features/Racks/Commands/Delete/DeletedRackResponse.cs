using NArchitecture.Core.Application.Responses;

namespace Application.Features.Racks.Commands.Delete;

public class DeletedRackResponse : IResponse
{
    public Guid Id { get; set; }
}