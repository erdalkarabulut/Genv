using FluentValidation;

namespace Application.Features.Donors.Commands.Update;

public class UpdateDonorCommandValidator : AbstractValidator<UpdateDonorCommand>
{
    public UpdateDonorCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.FullName).NotEmpty();
    }
}