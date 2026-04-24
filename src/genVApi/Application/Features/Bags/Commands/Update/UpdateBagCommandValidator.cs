using FluentValidation;

namespace Application.Features.Bags.Commands.Update;

public class UpdateBagCommandValidator : AbstractValidator<UpdateBagCommand>
{
    public UpdateBagCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.SessionId).NotEmpty();
    }
}