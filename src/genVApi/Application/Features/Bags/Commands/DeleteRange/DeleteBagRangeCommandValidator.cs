using FluentValidation;

namespace Application.Features.Bags.Commands.DeleteRange;

public class DeleteBagRangeCommandValidator : AbstractValidator<DeleteBagRangeCommand>
{
    public DeleteBagRangeCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty();
        RuleForEach(x => x.Ids).NotEmpty();
    }
}
