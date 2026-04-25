using FluentValidation;

namespace Application.Features.RackSlots.Commands.Update;

public class UpdateRackSlotCommandValidator : AbstractValidator<UpdateRackSlotCommand>
{
    public UpdateRackSlotCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.RackId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
    }
}
