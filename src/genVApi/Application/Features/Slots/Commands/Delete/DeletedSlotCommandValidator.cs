using FluentValidation;

namespace Application.Features.Slots.Commands.Delete;

public class DeleteSlotCommandValidator : AbstractValidator<DeleteSlotCommand>
{
    public DeleteSlotCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}