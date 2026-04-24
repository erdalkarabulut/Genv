using FluentValidation;

namespace Application.Features.Boxes.Commands.UpdateRange;

public class UpdateBoxRangeCommandValidator : AbstractValidator<UpdateBoxRangeCommand>
{
    public UpdateBoxRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id).NotEmpty();
            
                        item.RuleFor(i => i.RackId).NotEmpty();
        });
    }
}
