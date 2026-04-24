using NArchitecture.Core.Application.Responses;

namespace Application.Features.Tanks.Commands.Delete;

public class DeletedTankResponse : IResponse
{
    public Guid Id { get; set; }
}