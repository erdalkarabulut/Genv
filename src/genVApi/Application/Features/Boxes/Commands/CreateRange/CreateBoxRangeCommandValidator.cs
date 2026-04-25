using FluentValidation;

namespace Application.Features.Boxes.Commands.CreateRange;

public class CreateBoxRangeCommandValidator : AbstractValidator<CreateBoxRangeCommand>
{
    public CreateBoxRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            
                        item.RuleFor(i => i.SlotId).NotEmpty();
        });
    }
}
