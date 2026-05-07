using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;
using NArchitecture.Core.Security.Constants;

namespace Application.Features.PlcIntegration.Queries.GetListAlarmTemplate;

public class GetListAlarmTemplateQuery : IRequest<GetListResponse<AlarmTemplateListItemDto>>, ISecuredRequest, ILoggableRequest
{
    public PageRequest? PageRequest { get; set; }
    public string[] Roles => [GeneralOperationClaims.Admin];

    public class Handler : IRequestHandler<GetListAlarmTemplateQuery, GetListResponse<AlarmTemplateListItemDto>>
    {
        private readonly IPlcAlarmTemplateRepository _repository;

        public Handler(IPlcAlarmTemplateRepository repository) => _repository = repository;

        public async Task<GetListResponse<AlarmTemplateListItemDto>> Handle(GetListAlarmTemplateQuery request, CancellationToken cancellationToken)
        {
            IPaginate<PlcAlarmTemplate> page = await _repository.GetListAsync(
                predicate: null,
                orderBy: q => q.OrderByDescending(t => t.Name),
                include: null,
                index: request.PageRequest?.PageIndex ?? 0,
                size: request.PageRequest?.PageSize ?? int.MaxValue,
                enableTracking: false,
                cancellationToken: cancellationToken);

            return new GetListResponse<AlarmTemplateListItemDto>
            {
                Items = page.Items.Select(t => new AlarmTemplateListItemDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    SmsTemplate = t.SmsTemplate,
                    EmailSubjectTemplate = t.EmailSubjectTemplate,
                    EmailBodyTemplate = t.EmailBodyTemplate,
                    DevicePrefix = t.DevicePrefix,
                    IsActive = t.IsActive,
                }).ToList(),
                Index = page.Index,
                Size = page.Size,
                Count = page.Count,
                Pages = page.Pages,
            };
        }
    }
}

public sealed class AlarmTemplateListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string SmsTemplate { get; set; } = "";
    public string? EmailSubjectTemplate { get; set; }
    public string? EmailBodyTemplate { get; set; }
    public string? DevicePrefix { get; set; }
    public bool IsActive { get; set; }
}
