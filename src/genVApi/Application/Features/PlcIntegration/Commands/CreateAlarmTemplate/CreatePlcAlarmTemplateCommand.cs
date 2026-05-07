using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Security.Constants;

namespace Application.Features.PlcIntegration.Commands.CreateAlarmTemplate;

public class CreatePlcAlarmTemplateCommand : IRequest<CreatedPlcAlarmTemplateResponse>, ISecuredRequest, ILoggableRequest
{
    public string Name { get; set; } = "";
    public string SmsTemplate { get; set; } = "";
    public string? EmailSubjectTemplate { get; set; }
    public string? EmailBodyTemplate { get; set; }
    public string? DevicePrefix { get; set; }
    public bool IsActive { get; set; } = true;

    public string[] Roles => [GeneralOperationClaims.Admin];

    public class Handler : IRequestHandler<CreatePlcAlarmTemplateCommand, CreatedPlcAlarmTemplateResponse>
    {
        private readonly IPlcAlarmTemplateRepository _repository;

        public Handler(IPlcAlarmTemplateRepository repository) => _repository = repository;

        public async Task<CreatedPlcAlarmTemplateResponse> Handle(CreatePlcAlarmTemplateCommand request, CancellationToken cancellationToken)
        {
            var entity = new PlcAlarmTemplate
            {
                Name = request.Name.Trim(),
                SmsTemplate = request.SmsTemplate.Trim(),
                EmailSubjectTemplate = string.IsNullOrWhiteSpace(request.EmailSubjectTemplate) ? null : request.EmailSubjectTemplate.Trim(),
                EmailBodyTemplate = string.IsNullOrWhiteSpace(request.EmailBodyTemplate) ? null : request.EmailBodyTemplate.Trim(),
                DevicePrefix = string.IsNullOrWhiteSpace(request.DevicePrefix) ? null : request.DevicePrefix.Trim(),
                IsActive = request.IsActive,
            };

            PlcAlarmTemplate created = await _repository.AddAsync(entity);
            return new CreatedPlcAlarmTemplateResponse { Id = created.Id };
        }
    }
}

public sealed class CreatedPlcAlarmTemplateResponse
{
    public Guid Id { get; set; }
}
