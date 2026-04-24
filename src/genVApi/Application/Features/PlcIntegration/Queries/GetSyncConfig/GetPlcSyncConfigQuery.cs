using Application.Features.PlcIntegration.Dtos;
using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.PlcIntegration.Queries.GetSyncConfig;

public class GetPlcSyncConfigQuery : IRequest<List<PlcSensorSyncDto>>
{
    public class Handler : IRequestHandler<GetPlcSyncConfigQuery, List<PlcSensorSyncDto>>
    {
        private readonly IPlcSensorPointRepository _points;

        public Handler(IPlcSensorPointRepository points) => _points = points;

        public async Task<List<PlcSensorSyncDto>> Handle(GetPlcSyncConfigQuery request, CancellationToken cancellationToken)
        {
            IPaginate<PlcSensorPoint> page = await _points.GetListAsync(
                include: null,
                index: 0,
                size: int.MaxValue,
                enableTracking: false,
                cancellationToken: cancellationToken);

            return page.Items.Select(p => new PlcSensorSyncDto
            {
                Id = p.Id,
                SensorCode = p.SensorCode,
                DeviceName = p.DeviceName,
                DevicePrefix = p.DevicePrefix,
                DataLabel = p.DataLabel,
                ModbusHost = p.ModbusHost,
                ModbusPort = p.ModbusPort,
                SlaveId = p.SlaveId,
                RegisterAddress = p.RegisterAddress,
                RegisterLength = p.RegisterLength,
                ScaleDivisor = p.ScaleDivisor,
                PollIntervalSeconds = p.PollIntervalSeconds,
            }).ToList();
        }
    }
}
