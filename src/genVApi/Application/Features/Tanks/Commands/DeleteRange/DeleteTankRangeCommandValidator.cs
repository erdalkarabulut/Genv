using FluentValidation;

namespace Application.Features.Tanks.Commands.DeleteRange;

public class DeleteTankRangeCommandValidator : AbstractValidator<DeleteTankRangeCommand>
{
    public DeleteTankRangeCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty();
        RuleForEach(x => x.Ids).NotEmpty();
    }
}
