using FluentValidation;

namespace Application.Features.RackSlots.Commands.Delete;

public class DeleteRackSlotCommandValidator : AbstractValidator<DeleteRackSlotCommand>
{
    public DeleteRackSlotCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
