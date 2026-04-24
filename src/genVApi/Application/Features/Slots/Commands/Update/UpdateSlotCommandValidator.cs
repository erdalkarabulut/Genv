using FluentValidation;

namespace Application.Features.Slots.Commands.Update;

public class UpdateSlotCommandValidator : AbstractValidator<UpdateSlotCommand>
{
    public UpdateSlotCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.BoxId).NotEmpty();
        RuleFor(c => c.Position).NotEmpty();
    }
}