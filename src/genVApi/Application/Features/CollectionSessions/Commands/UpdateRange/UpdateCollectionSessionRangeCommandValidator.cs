using FluentValidation;

namespace Application.Features.CollectionSessions.Commands.UpdateRange;

public class UpdateCollectionSessionRangeCommandValidator : AbstractValidator<UpdateCollectionSessionRangeCommand>
{
    public UpdateCollectionSessionRangeCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id).NotEmpty();
            
                        item.RuleFor(i => i.PatientId).NotEmpty();
        });
    }
}
