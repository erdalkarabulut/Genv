using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Security.Constants;

namespace Application.Features.PlcIntegration.Commands.UpdateAlarmContact;

public class UpdatePlcAlarmContactCommand : IRequest<UpdatedPlcAlarmContactResponse>, ISecuredRequest, ILoggableRequest
{
    public Guid Id { get; set; }

    public string? DevicePrefix { get; set; }

    public Guid? AlarmTemplateId { get; set; }

    public string DisplayName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public bool SmsEnabled { get; set; }
    public bool EmailEnabled { get; set; }

    public string[] Roles => [GeneralOperationClaims.Admin];

    public class Handler : IRequestHandler<UpdatePlcAlarmContactCommand, UpdatedPlcAlarmContactResponse>
    {
        private readonly IPlcAlarmContactRepository _repository;

        public Handler(IPlcAlarmContactRepository repository) => _repository = repository;

        public async Task<UpdatedPlcAlarmContactResponse> Handle(UpdatePlcAlarmContactCommand request, CancellationToken cancellationToken)
        {
            PlcAlarmContact? entity = await _repository.GetAsync(c => c.Id == request.Id, cancellationToken: cancellationToken);
            if (entity is null)
                throw new BusinessException("Bildirim kaydı bulunamadı.");

            string? prefix =
                string.IsNullOrWhiteSpace(request.DevicePrefix) ? null : request.DevicePrefix.Trim();

            entity.DevicePrefix = prefix;
            entity.AlarmTemplateId = request.AlarmTemplateId;
            entity.DisplayName = request.DisplayName.Trim();
            entity.Phone = request.Phone.Trim();
            entity.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            entity.SmsEnabled = request.SmsEnabled;
            entity.EmailEnabled = request.EmailEnabled;

            await _repository.UpdateAsync(entity);

            return new UpdatedPlcAlarmContactResponse { Id = entity.Id };
        }
    }
}
