using FluentValidation;

namespace Application.Features.Bags.Commands.Delete;

public class DeleteBagCommandValidator : AbstractValidator<DeleteBagCommand>
{
    public DeleteBagCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}