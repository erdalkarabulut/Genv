using FluentValidation;

namespace Application.Features.PlcIntegration.Commands.CreateAlarmContact;

public class CreatePlcAlarmContactCommandValidator : AbstractValidator<CreatePlcAlarmContactCommand>
{
    public CreatePlcAlarmContactCommandValidator()
    {
        RuleFor(x => x.DevicePrefix).MaximumLength(32).When(x => !string.IsNullOrEmpty(x.DevicePrefix));
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Email).MaximumLength(256).When(x => !string.IsNullOrEmpty(x.Email));
    }
}
