using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Persistence.Paging;
using NArchitecture.Core.Security.Constants;

namespace Application.Features.PlcIntegration.Queries.GetListPlcSensorPoints;

public class GetListPlcSensorPointsQuery : IRequest<List<GetListPlcSensorPointListItemDto>>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.Admin];

    public class Handler : IRequestHandler<GetListPlcSensorPointsQuery, List<GetListPlcSensorPointListItemDto>>
    {
        private readonly IPlcSensorPointRepository _repository;

        public Handler(IPlcSensorPointRepository repository) => _repository = repository;

        public async Task<List<GetListPlcSensorPointListItemDto>> Handle(
            GetListPlcSensorPointsQuery request,
            CancellationToken cancellationToken
        )
        {
            IPaginate<PlcSensorPoint> page = await _repository.GetListAsync(
                index: 0,
                size: int.MaxValue,
                cancellationToken: cancellationToken
            );

            return page.Items
                .Select(p => new GetListPlcSensorPointListItemDto
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
                    AlarmLow = p.AlarmLow,
                    AlarmHigh = p.AlarmHigh,
                    AlarmActive = p.AlarmActive,
                })
                .OrderBy(x => x.DeviceName)
                .ThenBy(x => x.SensorCode)
                .ToList();
        }
    }
}
