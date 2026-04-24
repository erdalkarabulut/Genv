using FluentValidation;

namespace Application.Features.Racks.Commands.Update;

public class UpdateRackCommandValidator : AbstractValidator<UpdateRackCommand>
{
    public UpdateRackCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.TankId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
    }
}