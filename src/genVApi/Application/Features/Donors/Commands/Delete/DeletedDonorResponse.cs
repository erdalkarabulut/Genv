using NArchitecture.Core.Application.Responses;

namespace Application.Features.Donors.Commands.Delete;

public class DeletedDonorResponse : IResponse
{
    public Guid Id { get; set; }
}