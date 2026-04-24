using FluentValidation;

namespace Application.Features.Patients.Commands.UpdateRange;

public class UpdatePatientRangeCommandValidator : AbstractValidator<UpdatePatientRangeCommand>
{
    public UpdatePatientRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id).NotEmpty();
            
                        item.RuleFor(i => i.FullName).NotEmpty();
        });
    }
}
