using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Security.Constants;

namespace Application.Features.PlcIntegration.Commands.UpdateSensorPoint;

public class UpdatePlcSensorPointCommand : IRequest<UpdatedPlcSensorPointResponse>, ISecuredRequest, ILoggableRequest
{
    public Guid Id { get; set; }
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

    public class Handler : IRequestHandler<UpdatePlcSensorPointCommand, UpdatedPlcSensorPointResponse>
    {
        private readonly IPlcSensorPointRepository _repository;

        public Handler(IPlcSensorPointRepository repository) => _repository = repository;

        public async Task<UpdatedPlcSensorPointResponse> Handle(UpdatePlcSensorPointCommand request, CancellationToken cancellationToken)
        {
            PlcSensorPoint? entity = await _repository.GetAsync(p => p.Id == request.Id, cancellationToken: cancellationToken);
            if (entity is null)
                throw new BusinessException("Sensör kaydı bulunamadı.");

            string code = request.SensorCode.Trim();
            bool duplicate = await _repository.AnyAsync(
                p => p.SensorCode == code && p.Id != request.Id,
                cancellationToken: cancellationToken
            );
            if (duplicate)
                throw new BusinessException("Bu sensör kodu başka bir kayıtta kullanılıyor.");

            entity.SensorCode = code;
            entity.DeviceName = request.DeviceName.Trim();
            entity.DevicePrefix = request.DevicePrefix.Trim();
            entity.DataLabel = request.DataLabel.Trim();
            entity.ModbusHost = request.ModbusHost.Trim();
            entity.ModbusPort = request.ModbusPort;
            entity.SlaveId = request.SlaveId;
            entity.RegisterAddress = request.RegisterAddress;
            entity.RegisterLength = request.RegisterLength;
            entity.ScaleDivisor = request.ScaleDivisor <= 0 ? 1 : request.ScaleDivisor;
            entity.PollIntervalSeconds = request.PollIntervalSeconds <= 0 ? 5 : request.PollIntervalSeconds;
            entity.AlarmLow = request.AlarmLow;
            entity.AlarmHigh = request.AlarmHigh;
            entity.AlarmActive = request.AlarmActive;

            await _repository.UpdateAsync(entity);

            return new UpdatedPlcSensorPointResponse { Id = entity.Id };
        }
    }
}
