using FluentValidation;

namespace Application.Features.Donors.Commands.Create;

public class CreateDonorCommandValidator : AbstractValidator<CreateDonorCommand>
{
    public CreateDonorCommandValidator()
    {
        RuleFor(c => c.FullName).NotEmpty();
    }
}