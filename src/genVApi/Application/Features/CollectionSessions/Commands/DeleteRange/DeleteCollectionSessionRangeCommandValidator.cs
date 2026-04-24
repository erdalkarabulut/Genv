using FluentValidation;

namespace Application.Features.CollectionSessions.Commands.DeleteRange;

public class DeleteCollectionSessionRangeCommandValidator : AbstractValidator<DeleteCollectionSessionRangeCommand>
{
    public DeleteCollectionSessionRangeCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty();
        RuleForEach(x => x.Ids).NotEmpty();
    }
}
