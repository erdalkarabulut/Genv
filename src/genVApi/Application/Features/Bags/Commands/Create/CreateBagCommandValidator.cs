using FluentValidation;

namespace Application.Features.Bags.Commands.Create;

public class CreateBagCommandValidator : AbstractValidator<CreateBagCommand>
{
    public CreateBagCommandValidator()
    {
        RuleFor(c => c.SessionId).NotEmpty();
    }
}