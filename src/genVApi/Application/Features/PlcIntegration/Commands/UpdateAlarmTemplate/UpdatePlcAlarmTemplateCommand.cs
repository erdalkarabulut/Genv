using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Security.Constants;

namespace Application.Features.PlcIntegration.Commands.UpdateAlarmTemplate;

public class UpdatePlcAlarmTemplateCommand : IRequest<UpdatedPlcAlarmTemplateResponse>, ISecuredRequest, ILoggableRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string SmsTemplate { get; set; } = "";
    public string? EmailSubjectTemplate { get; set; }
    public string? EmailBodyTemplate { get; set; }
    public string? DevicePrefix { get; set; }
    public bool IsActive { get; set; }

    public string[] Roles => [GeneralOperationClaims.Admin];

    public class Handler : IRequestHandler<UpdatePlcAlarmTemplateCommand, UpdatedPlcAlarmTemplateResponse>
    {
        private readonly IPlcAlarmTemplateRepository _repository;

        public Handler(IPlcAlarmTemplateRepository repository) => _repository = repository;

        public async Task<UpdatedPlcAlarmTemplateResponse> Handle(UpdatePlcAlarmTemplateCommand request, CancellationToken cancellationToken)
        {
            PlcAlarmTemplate? entity = await _repository.GetAsync(t => t.Id == request.Id, cancellationToken: cancellationToken);
            if (entity is null)
                throw new BusinessException("Alarm template bulunamadı.");

            entity.Name = request.Name.Trim();
            entity.SmsTemplate = request.SmsTemplate.Trim();
            entity.EmailSubjectTemplate = string.IsNullOrWhiteSpace(request.EmailSubjectTemplate) ? null : request.EmailSubjectTemplate.Trim();
            entity.EmailBodyTemplate = string.IsNullOrWhiteSpace(request.EmailBodyTemplate) ? null : request.EmailBodyTemplate.Trim();
            entity.DevicePrefix = string.IsNullOrWhiteSpace(request.DevicePrefix) ? null : request.DevicePrefix.Trim();
            entity.IsActive = request.IsActive;

            await _repository.UpdateAsync(entity);
            return new UpdatedPlcAlarmTemplateResponse();
        }
    }
}

public sealed class UpdatedPlcAlarmTemplateResponse
{
}
