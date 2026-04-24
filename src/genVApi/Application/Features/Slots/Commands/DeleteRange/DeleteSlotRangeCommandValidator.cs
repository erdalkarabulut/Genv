using FluentValidation;

namespace Application.Features.Slots.Commands.DeleteRange;

public class DeleteSlotRangeCommandValidator : AbstractValidator<DeleteSlotRangeCommand>
{
    public DeleteSlotRangeCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty();
        RuleForEach(x => x.Ids).NotEmpty();
    }
}
