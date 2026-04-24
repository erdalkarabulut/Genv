using NArchitecture.Core.Application.Responses;

namespace Application.Features.BagMovements.Commands.Delete;

public class DeletedBagMovementResponse : IResponse
{
    public Guid Id { get; set; }
}