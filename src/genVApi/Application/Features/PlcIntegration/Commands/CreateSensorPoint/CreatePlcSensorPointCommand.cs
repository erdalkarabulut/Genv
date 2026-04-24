using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Security.Constants;

namespace Application.Features.PlcIntegration.Commands.CreateSensorPoint;

public class CreatePlcSensorPointCommand : IRequest<CreatedPlcSensorPointResponse>, ISecuredRequest, ILoggableRequest
{
    public string SensorCode { get; set; } = "";
    public string DeviceName { get; set; } = "";
    public string DevicePrefix { get; set; } = "";
    public string DataLabel { get; set; } = "";
    public string ModbusHost { get; set; } = "";
    public int ModbusPort { get; set; } = 502;
    public int SlaveId { get; set; }
    public int RegisterAddress { get; set; }
    public int RegisterLength { get; set; } = 1;
    public double ScaleDivisor { get; set; } = 1;
    public int PollIntervalSeconds { get; set; } = 5;
    public double? AlarmLow { get; set; }
    public double? AlarmHigh { get; set; }
    public bool AlarmActive { get; set; }

    public string[] Roles => [GeneralOperationClaims.Admin];

    public class Handler : IRequestHandler<CreatePlcSensorPointCommand, CreatedPlcSensorPointResponse>
    {
        private readonly IPlcSensorPointRepository _repository;

        public Handler(IPlcSensorPointRepository repository) => _repository = repository;

        public async Task<CreatedPlcSensorPointResponse> Handle(CreatePlcSensorPointCommand request, CancellationToken cancellationToken)
        {
            string sensorCode = request.SensorCode.Trim();
            bool duplicate = await _repository.AnyAsync(p => p.SensorCode == sensorCode, cancellationToken: cancellationToken);
            if (duplicate)
                throw new BusinessException("Bu sensör kodu zaten kullanılıyor.");

            var entity = new PlcSensorPoint
            {
                SensorCode = sensorCode,
                DeviceName = request.DeviceName.Trim(),
                DevicePrefix = request.DevicePrefix.Trim(),
                DataLabel = request.DataLabel.Trim(),
                ModbusHost = request.ModbusHost.Trim(),
                ModbusPort = request.ModbusPort,
                SlaveId = request.SlaveId,
                RegisterAddress = request.RegisterAddress,
                RegisterLength = request.RegisterLength,
                ScaleDivisor = request.ScaleDivisor <= 0 ? 1 : request.ScaleDivisor,
                PollIntervalSeconds = request.PollIntervalSeconds <= 0 ? 5 : request.PollIntervalSeconds,
                AlarmLow = request.AlarmLow,
                AlarmHigh = request.AlarmHigh,
                AlarmActive = request.AlarmActive,
            };

            PlcSensorPoint created = await _repository.AddAsync(entity);
            return new CreatedPlcSensorPointResponse { Id = created.Id };
        }
    }
}

public sealed class CreatedPlcSensorPointResponse
{
    public Guid Id { get; set; }
}
