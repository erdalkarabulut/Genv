using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;
using NArchitecture.Core.Security.Constants;

namespace Application.Features.PlcIntegration.Queries.GetListPlcAlarmContacts;

public class GetListPlcAlarmContactsQuery : IRequest<GetListResponse<GetListPlcAlarmContactListItemDto>>, ISecuredRequest
{
    public PageRequest? PageRequest { get; set; }
    public string[] Roles => [GeneralOperationClaims.Admin];

    public class Handler : IRequestHandler<GetListPlcAlarmContactsQuery, GetListResponse<GetListPlcAlarmContactListItemDto>>
    {
        private readonly IPlcAlarmContactRepository _repository;

        public Handler(IPlcAlarmContactRepository repository) => _repository = repository;

        public async Task<GetListResponse<GetListPlcAlarmContactListItemDto>> Handle(
            GetListPlcAlarmContactsQuery request,
            CancellationToken cancellationToken
        )
        {
            IPaginate<PlcAlarmContact> page = await _repository.GetListAsync(
                index: request.PageRequest?.PageIndex ?? 0,
                size: request.PageRequest?.PageSize ?? int.MaxValue,
                include: c => c.Include(x => x.AlarmTemplate),
                orderBy: q => q.OrderBy(x => x.DisplayName),
                cancellationToken: cancellationToken
            );

            return new GetListResponse<GetListPlcAlarmContactListItemDto>
            {
                Items = page.Items
                    .Select(c => new GetListPlcAlarmContactListItemDto
                    {
                        Id = c.Id,
                        DevicePrefix = c.DevicePrefix,
                        AlarmTemplateId = c.AlarmTemplateId,
                        AlarmTemplateName = c.AlarmTemplate?.Name,
                        DisplayName = c.DisplayName,
                        Phone = c.Phone,
                        Email = c.Email,
                        SmsEnabled = c.SmsEnabled,
                        EmailEnabled = c.EmailEnabled,
                    })
                    .ToList(),
                Index = page.Index,
                Size = page.Size,
                Count = page.Count,
                Pages = page.Pages,
            };
        }
    }
}
