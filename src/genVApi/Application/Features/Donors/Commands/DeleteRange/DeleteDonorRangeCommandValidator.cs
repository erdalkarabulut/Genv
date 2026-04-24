using FluentValidation;

namespace Application.Features.Donors.Commands.DeleteRange;

public class DeleteDonorRangeCommandValidator : AbstractValidator<DeleteDonorRangeCommand>
{
    public DeleteDonorRangeCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty();
        RuleForEach(x => x.Ids).NotEmpty();
    }
}
