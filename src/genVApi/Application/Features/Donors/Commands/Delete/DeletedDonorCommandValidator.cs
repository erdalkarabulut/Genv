using FluentValidation;

namespace Application.Features.Donors.Commands.Delete;

public class DeleteDonorCommandValidator : AbstractValidator<DeleteDonorCommand>
{
    public DeleteDonorCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}