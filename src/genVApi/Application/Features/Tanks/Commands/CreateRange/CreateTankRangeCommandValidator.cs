using FluentValidation;

namespace Application.Features.Tanks.Commands.CreateRange;

public class CreateTankRangeCommandValidator : AbstractValidator<CreateTankRangeCommand>
{
    public CreateTankRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            
                        item.RuleFor(i => i.Name).NotEmpty();
        });
    }
}
