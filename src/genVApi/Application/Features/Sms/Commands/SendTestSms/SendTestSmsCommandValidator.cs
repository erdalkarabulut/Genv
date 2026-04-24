using FluentValidation;

namespace Application.Features.Sms.Commands.SendTestSms;

public class SendTestSmsCommandValidator : AbstractValidator<SendTestSmsCommand>
{
    public SendTestSmsCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Message).NotEmpty().MinimumLength(1).MaximumLength(900);
    }
}
