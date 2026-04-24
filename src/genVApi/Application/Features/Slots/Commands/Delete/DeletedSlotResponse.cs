using NArchitecture.Core.Application.Responses;

namespace Application.Features.Slots.Commands.Delete;

public class DeletedSlotResponse : IResponse
{
    public Guid Id { get; set; }
}