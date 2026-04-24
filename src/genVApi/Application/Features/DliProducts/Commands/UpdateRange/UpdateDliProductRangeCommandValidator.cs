using FluentValidation;

namespace Application.Features.DliProducts.Commands.UpdateRange;

public class UpdateDliProductRangeCommandValidator : AbstractValidator<UpdateDliProductRangeCommand>
{
    public UpdateDliProductRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id).NotEmpty();
            
                        item.RuleFor(i => i.PatientId).NotEmpty();
        });
    }
}
