using NArchitecture.Core.Application.Responses;

namespace Application.Features.Boxes.Commands.Delete;

public class DeletedBoxResponse : IResponse
{
    public Guid Id { get; set; }
}