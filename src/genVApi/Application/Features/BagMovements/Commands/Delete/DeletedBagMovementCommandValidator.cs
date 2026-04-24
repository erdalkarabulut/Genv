using FluentValidation;

namespace Application.Features.BagMovements.Commands.Delete;

public class DeleteBagMovementCommandValidator : AbstractValidator<DeleteBagMovementCommand>
{
    public DeleteBagMovementCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}