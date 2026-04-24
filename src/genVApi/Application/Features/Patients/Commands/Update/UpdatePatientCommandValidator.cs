using FluentValidation;

namespace Application.Features.Patients.Commands.Update;

public class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.FullName).NotEmpty();
    }
}