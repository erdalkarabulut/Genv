using FluentValidation;

namespace Application.Features.Patients.Commands.DeleteRange;

public class DeletePatientRangeCommandValidator : AbstractValidator<DeletePatientRangeCommand>
{
    public DeletePatientRangeCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty();
        RuleForEach(x => x.Ids).NotEmpty();
    }
}
