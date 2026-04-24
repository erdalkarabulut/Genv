using FluentValidation;

namespace Application.Features.Boxes.Commands.Delete;

public class DeleteBoxCommandValidator : AbstractValidator<DeleteBoxCommand>
{
    public DeleteBoxCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}