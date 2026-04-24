using FluentValidation;

namespace Application.Features.Products.Commands.DeleteRange;

public class DeleteProductRangeCommandValidator : AbstractValidator<DeleteProductRangeCommand>
{
    public DeleteProductRangeCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty();
        RuleForEach(x => x.Ids).NotEmpty();
    }
}
