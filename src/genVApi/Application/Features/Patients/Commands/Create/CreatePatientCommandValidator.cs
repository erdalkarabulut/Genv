using FluentValidation;

namespace Application.Features.Patients.Commands.Create;

public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(c => c.FullName).NotEmpty();
    }
}