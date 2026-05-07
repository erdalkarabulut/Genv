using Application.Features.PlcIntegration.Commands.CreateAlarmTemplate;
using FluentValidation;

namespace Application.Features.PlcIntegration.Commands.CreateAlarmTemplate;

public class CreatePlcAlarmTemplateCommandValidator : AbstractValidator<CreatePlcAlarmTemplateCommand>
{
    public CreatePlcAlarmTemplateCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SmsTemplate).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.EmailSubjectTemplate).MaximumLength(200).When(x => !string.IsNullOrEmpty(x.EmailSubjectTemplate));
        RuleFor(x => x.EmailBodyTemplate).MaximumLength(4000).When(x => !string.IsNullOrEmpty(x.EmailBodyTemplate));
        RuleFor(x => x.DevicePrefix).MaximumLength(32).When(x => !string.IsNullOrEmpty(x.DevicePrefix));
    }
}
