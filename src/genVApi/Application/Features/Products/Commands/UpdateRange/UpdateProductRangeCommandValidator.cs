using FluentValidation;

namespace Application.Features.Products.Commands.UpdateRange;

public class UpdateProductRangeCommandValidator : AbstractValidator<UpdateProductRangeCommand>
{
    public UpdateProductRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id).NotEmpty();
            
                        item.RuleFor(i => i.SessionId).NotEmpty();
        });
    }
}
