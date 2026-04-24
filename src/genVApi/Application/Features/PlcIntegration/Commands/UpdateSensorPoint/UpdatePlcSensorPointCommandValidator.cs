using FluentValidation;

namespace Application.Features.PlcIntegration.Commands.UpdateSensorPoint;

public class UpdatePlcSensorPointCommandValidator : AbstractValidator<UpdatePlcSensorPointCommand>
{
    public UpdatePlcSensorPointCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.SensorCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.DeviceName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DevicePrefix).NotEmpty().MaximumLength(32);
        RuleFor(x => x.DataLabel).NotEmpty().MaximumLength(64);
        RuleFor(x => x.ModbusHost).NotEmpty().MaximumLength(128);
        RuleFor(x => x.ModbusPort).InclusiveBetween(1, 65535);
        RuleFor(x => x.SlaveId).InclusiveBetween(0, 255);
        RuleFor(x => x.RegisterAddress).InclusiveBetween(0, 65535);
        RuleFor(x => x.RegisterLength).InclusiveBetween(1, 125);
    }
}
