using FluentValidation;

namespace Application.Features.Slots.Commands.UpdateRange;

public class UpdateSlotRangeCommandValidator : AbstractValidator<UpdateSlotRangeCommand>
{
    public UpdateSlotRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id).NotEmpty();
            
                        item.RuleFor(i => i.BoxId).NotEmpty();
        });
    }
}
