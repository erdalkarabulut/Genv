using NArchitecture.Core.Application.Responses;

namespace Application.Features.Bags.Commands.Delete;

public class DeletedBagResponse : IResponse
{
    public Guid Id { get; set; }
}