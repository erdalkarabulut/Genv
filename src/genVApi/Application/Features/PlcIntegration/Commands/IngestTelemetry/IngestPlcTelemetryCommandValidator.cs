using FluentValidation;

namespace Application.Features.PlcIntegration.Commands.IngestTelemetry;

public class IngestPlcTelemetryCommandValidator : AbstractValidator<IngestPlcTelemetryCommand>
{
    public IngestPlcTelemetryCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.SensorCode).NotEmpty().MaximumLength(64);
        });
    }
}
