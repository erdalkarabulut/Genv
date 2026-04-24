using FluentValidation;

namespace Application.Features.Racks.Commands.CreateRange;

public class CreateRackRangeCommandValidator : AbstractValidator<CreateRackRangeCommand>
{
    public CreateRackRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            
                        item.RuleFor(i => i.TankId).NotEmpty();
        });
    }
}
