using FluentValidation;

namespace Application.Features.DliProducts.Commands.Delete;

public class DeleteDliProductCommandValidator : AbstractValidator<DeleteDliProductCommand>
{
    public DeleteDliProductCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}