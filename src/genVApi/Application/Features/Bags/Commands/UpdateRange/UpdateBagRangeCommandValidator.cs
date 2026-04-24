using FluentValidation;

namespace Application.Features.Bags.Commands.UpdateRange;

public class UpdateBagRangeCommandValidator : AbstractValidator<UpdateBagRangeCommand>
{
    public UpdateBagRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id).NotEmpty();
            
                        item.RuleFor(i => i.SessionId).NotEmpty();
        });
    }
}
