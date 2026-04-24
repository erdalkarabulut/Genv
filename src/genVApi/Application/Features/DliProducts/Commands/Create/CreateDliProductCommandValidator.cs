using FluentValidation;

namespace Application.Features.DliProducts.Commands.Create;

public class CreateDliProductCommandValidator : AbstractValidator<CreateDliProductCommand>
{
    public CreateDliProductCommandValidator()
    {
        RuleFor(c => c.PatientId).NotEmpty();
    }
}