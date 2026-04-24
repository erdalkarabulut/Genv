using FluentValidation;

namespace Application.Features.Donors.Commands.CreateRange;

public class CreateDonorRangeCommandValidator : AbstractValidator<CreateDonorRangeCommand>
{
    public CreateDonorRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            
                        item.RuleFor(i => i.FullName).NotEmpty();
        });
    }
}
