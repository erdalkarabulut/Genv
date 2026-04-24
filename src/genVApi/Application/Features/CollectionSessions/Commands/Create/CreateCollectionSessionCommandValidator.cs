using FluentValidation;

namespace Application.Features.CollectionSessions.Commands.Create;

public class CreateCollectionSessionCommandValidator : AbstractValidator<CreateCollectionSessionCommand>
{
    public CreateCollectionSessionCommandValidator()
    {
        RuleFor(c => c.PatientId).NotEmpty();
    }
}