using FluentValidation;

namespace Application.Features.Tanks.Commands.Delete;

public class DeleteTankCommandValidator : AbstractValidator<DeleteTankCommand>
{
    public DeleteTankCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}