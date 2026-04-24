using FluentValidation;

namespace Application.Features.Donors.Commands.UpdateRange;

public class UpdateDonorRangeCommandValidator : AbstractValidator<UpdateDonorRangeCommand>
{
    public UpdateDonorRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id).NotEmpty();
            
                        item.RuleFor(i => i.FullName).NotEmpty();
        });
    }
}
