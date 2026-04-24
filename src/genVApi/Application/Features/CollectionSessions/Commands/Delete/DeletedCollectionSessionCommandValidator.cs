using FluentValidation;

namespace Application.Features.CollectionSessions.Commands.Delete;

public class DeleteCollectionSessionCommandValidator : AbstractValidator<DeleteCollectionSessionCommand>
{
    public DeleteCollectionSessionCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}