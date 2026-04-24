using FluentValidation;

namespace Application.Features.CollectionSessions.Commands.CreateRange;

public class CreateCollectionSessionRangeCommandValidator : AbstractValidator<CreateCollectionSessionRangeCommand>
{
    public CreateCollectionSessionRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            
                        item.RuleFor(i => i.PatientId).NotEmpty();
        });
    }
}
