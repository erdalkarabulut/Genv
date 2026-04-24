using FluentValidation;

namespace Application.Features.Patients.Commands.CreateRange;

public class CreatePatientRangeCommandValidator : AbstractValidator<CreatePatientRangeCommand>
{
    public CreatePatientRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            
                        item.RuleFor(i => i.FullName).NotEmpty();
        });
    }
}
