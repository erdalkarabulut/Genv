using FluentValidation;

namespace Application.Features.Boxes.Commands.Create;

public class CreateBoxCommandValidator : AbstractValidator<CreateBoxCommand>
{
    public CreateBoxCommandValidator()
    {
        RuleFor(c => c.SlotId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
    }
}