using FluentValidation;

namespace Application.Features.Racks.Commands.Delete;

public class DeleteRackCommandValidator : AbstractValidator<DeleteRackCommand>
{
    public DeleteRackCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}