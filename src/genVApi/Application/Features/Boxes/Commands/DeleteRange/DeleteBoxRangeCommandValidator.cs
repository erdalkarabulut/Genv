using FluentValidation;

namespace Application.Features.Boxes.Commands.DeleteRange;

public class DeleteBoxRangeCommandValidator : AbstractValidator<DeleteBoxRangeCommand>
{
    public DeleteBoxRangeCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty();
        RuleForEach(x => x.Ids).NotEmpty();
    }
}
