using FluentValidation;

namespace Application.Features.BagMovements.Commands.CreateRange;

public class CreateBagMovementRangeCommandValidator : AbstractValidator<CreateBagMovementRangeCommand>
{
    public CreateBagMovementRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            
                        item.RuleFor(i => i.BagId).NotEmpty();
        });
    }
}
