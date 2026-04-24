using FluentValidation;

namespace Application.Features.Slots.Commands.CreateRange;

public class CreateSlotRangeCommandValidator : AbstractValidator<CreateSlotRangeCommand>
{
    public CreateSlotRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            
                        item.RuleFor(i => i.BoxId).NotEmpty();
        });
    }
}
