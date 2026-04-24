using FluentValidation;

namespace Application.Features.Tanks.Commands.Create;

public class CreateTankCommandValidator : AbstractValidator<CreateTankCommand>
{
    public CreateTankCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty();
    }
}