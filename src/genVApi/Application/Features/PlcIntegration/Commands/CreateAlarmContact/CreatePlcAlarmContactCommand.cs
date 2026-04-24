using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Security.Constants;

namespace Application.Features.PlcIntegration.Commands.CreateAlarmContact;

public class CreatePlcAlarmContactCommand : IRequest<CreatedPlcAlarmContactResponse>, ISecuredRequest, ILoggableRequest
{
    /// <summary>Boş veya null = tüm cihaz önekleri için geçerli.</summary>
    public string? DevicePrefix { get; set; }

    public string DisplayName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public bool SmsEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; }

    public string[] Roles => [GeneralOperationClaims.Admin];

    public class Handler : IRequestHandler<CreatePlcAlarmContactCommand, CreatedPlcAlarmContactResponse>
    {
        private readonly IPlcAlarmContactRepository _repository;

        public Handler(IPlcAlarmContactRepository repository) => _repository = repository;

        public async Task<CreatedPlcAlarmContactResponse> Handle(CreatePlcAlarmContactCommand request, CancellationToken cancellationToken)
        {
            string? prefix =
                string.IsNullOrWhiteSpace(request.DevicePrefix) ? null : request.DevicePrefix.Trim();

            var entity = new PlcAlarmContact
            {
                DevicePrefix = prefix,
                DisplayName = request.DisplayName.Trim(),
                Phone = request.Phone.Trim(),
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                SmsEnabled = request.SmsEnabled,
                EmailEnabled = request.EmailEnabled,
            };

            PlcAlarmContact created = await _repository.AddAsync(entity);
            return new CreatedPlcAlarmContactResponse { Id = created.Id };
        }
    }
}
