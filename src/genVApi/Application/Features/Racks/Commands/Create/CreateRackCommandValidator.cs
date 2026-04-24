using FluentValidation;

namespace Application.Features.Racks.Commands.Create;

public class CreateRackCommandValidator : AbstractValidator<CreateRackCommand>
{
    public CreateRackCommandValidator()
    {
        RuleFor(c => c.TankId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
    }
}