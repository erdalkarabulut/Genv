using FluentValidation;

namespace Application.Features.DliProducts.Commands.DeleteRange;

public class DeleteDliProductRangeCommandValidator : AbstractValidator<DeleteDliProductRangeCommand>
{
    public DeleteDliProductRangeCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty();
        RuleForEach(x => x.Ids).NotEmpty();
    }
}
