using FluentValidation;

namespace Application.Features.Slots.Commands.Create;

public class CreateSlotCommandValidator : AbstractValidator<CreateSlotCommand>
{
    public CreateSlotCommandValidator()
    {
        RuleFor(c => c.BoxId).NotEmpty();
        RuleFor(c => c.Position).NotEmpty();
    }
}