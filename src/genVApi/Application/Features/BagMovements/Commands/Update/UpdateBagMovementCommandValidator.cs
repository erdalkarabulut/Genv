using FluentValidation;

namespace Application.Features.BagMovements.Commands.Update;

public class UpdateBagMovementCommandValidator : AbstractValidator<UpdateBagMovementCommand>
{
    public UpdateBagMovementCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.BagId).NotEmpty();
        RuleFor(c => c.Action).NotEmpty();
    }
}