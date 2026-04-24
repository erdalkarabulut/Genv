using FluentValidation;

namespace Application.Features.BagMovements.Commands.DeleteRange;

public class DeleteBagMovementRangeCommandValidator : AbstractValidator<DeleteBagMovementRangeCommand>
{
    public DeleteBagMovementRangeCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty();
        RuleForEach(x => x.Ids).NotEmpty();
    }
}
