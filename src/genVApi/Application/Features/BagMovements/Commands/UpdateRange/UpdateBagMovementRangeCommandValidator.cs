using FluentValidation;

namespace Application.Features.BagMovements.Commands.UpdateRange;

public class UpdateBagMovementRangeCommandValidator : AbstractValidator<UpdateBagMovementRangeCommand>
{
    public UpdateBagMovementRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id).NotEmpty();
            
                        item.RuleFor(i => i.BagId).NotEmpty();
        });
    }
}
