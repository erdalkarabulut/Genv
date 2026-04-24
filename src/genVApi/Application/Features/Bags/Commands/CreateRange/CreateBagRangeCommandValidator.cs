using FluentValidation;

namespace Application.Features.Bags.Commands.CreateRange;

public class CreateBagRangeCommandValidator : AbstractValidator<CreateBagRangeCommand>
{
    public CreateBagRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            
                        item.RuleFor(i => i.SessionId).NotEmpty();
        });
    }
}
