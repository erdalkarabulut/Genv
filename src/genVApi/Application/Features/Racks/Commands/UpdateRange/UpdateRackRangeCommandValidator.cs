using FluentValidation;

namespace Application.Features.Racks.Commands.UpdateRange;

public class UpdateRackRangeCommandValidator : AbstractValidator<UpdateRackRangeCommand>
{
    public UpdateRackRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id).NotEmpty();
            
                        item.RuleFor(i => i.TankId).NotEmpty();
        });
    }
}
