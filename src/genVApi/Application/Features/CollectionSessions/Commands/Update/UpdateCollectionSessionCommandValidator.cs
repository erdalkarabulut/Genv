using FluentValidation;

namespace Application.Features.CollectionSessions.Commands.Update;

public class UpdateCollectionSessionCommandValidator : AbstractValidator<UpdateCollectionSessionCommand>
{
    public UpdateCollectionSessionCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.PatientId).NotEmpty();
    }
}