using FluentValidation;

namespace Application.Features.DliProducts.Commands.Update;

public class UpdateDliProductCommandValidator : AbstractValidator<UpdateDliProductCommand>
{
    public UpdateDliProductCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.PatientId).NotEmpty();
    }
}