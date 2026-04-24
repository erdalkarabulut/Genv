using FluentValidation;

namespace Application.Features.Racks.Commands.DeleteRange;

public class DeleteRackRangeCommandValidator : AbstractValidator<DeleteRackRangeCommand>
{
    public DeleteRackRangeCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty();
        RuleForEach(x => x.Ids).NotEmpty();
    }
}
