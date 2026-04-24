using NArchitecture.Core.Application.Responses;

namespace Application.Features.DliProducts.Commands.Delete;

public class DeletedDliProductResponse : IResponse
{
    public Guid Id { get; set; }
}