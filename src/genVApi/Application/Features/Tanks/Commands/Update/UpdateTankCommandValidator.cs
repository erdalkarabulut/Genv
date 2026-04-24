using FluentValidation;

namespace Application.Features.Tanks.Commands.Update;

public class UpdateTankCommandValidator : AbstractValidator<UpdateTankCommand>
{
    public UpdateTankCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
    }
}