using FluentValidation;

namespace Application.Features.Boxes.Commands.Update;

public class UpdateBoxCommandValidator : AbstractValidator<UpdateBoxCommand>
{
    public UpdateBoxCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.SlotId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
    }
}