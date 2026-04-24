using FluentValidation;

namespace Application.Features.Products.Commands.CreateRange;

public class CreateProductRangeCommandValidator : AbstractValidator<CreateProductRangeCommand>
{
    public CreateProductRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            
                        item.RuleFor(i => i.SessionId).NotEmpty();
        });
    }
}
