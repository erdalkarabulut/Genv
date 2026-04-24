using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Persistence.Paging;
using NArchitecture.Core.Security.Constants;

namespace Application.Features.PlcIntegration.Queries.GetListPlcAlarmContacts;

public class GetListPlcAlarmContactsQuery : IRequest<List<GetListPlcAlarmContactListItemDto>>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.Admin];

    public class Handler : IRequestHandler<GetListPlcAlarmContactsQuery, List<GetListPlcAlarmContactListItemDto>>
    {
        private readonly IPlcAlarmContactRepository _repository;

        public Handler(IPlcAlarmContactRepository repository) => _repository = repository;

        public async Task<List<GetListPlcAlarmContactListItemDto>> Handle(
            GetListPlcAlarmContactsQuery request,
            CancellationToken cancellationToken
        )
        {
            IPaginate<PlcAlarmContact> page = await _repository.GetListAsync(
                index: 0,
                size: int.MaxValue,
                cancellationToken: cancellationToken
            );

            return page.Items
                .Select(c => new GetListPlcAlarmContactListItemDto
                {
                    Id = c.Id,
                    DevicePrefix = c.DevicePrefix,
                    DisplayName = c.DisplayName,
                    Phone = c.Phone,
                    Email = c.Email,
                    SmsEnabled = c.SmsEnabled,
                    EmailEnabled = c.EmailEnabled,
                })
                .OrderBy(x => x.DisplayName)
                .ToList();
        }
    }
}
