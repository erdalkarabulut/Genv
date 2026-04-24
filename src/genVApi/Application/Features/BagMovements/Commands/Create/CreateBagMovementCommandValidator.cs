using FluentValidation;

namespace Application.Features.BagMovements.Commands.Create;

public class CreateBagMovementCommandValidator : AbstractValidator<CreateBagMovementCommand>
{
    public CreateBagMovementCommandValidator()
    {
        RuleFor(c => c.BagId).NotEmpty();
        RuleFor(c => c.Action).NotEmpty();
    }
}