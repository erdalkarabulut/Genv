using FluentValidation;

namespace Application.Features.DliProducts.Commands.CreateRange;

public class CreateDliProductRangeCommandValidator : AbstractValidator<CreateDliProductRangeCommand>
{
    public CreateDliProductRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            
                        item.RuleFor(i => i.PatientId).NotEmpty();
        });
    }
}
