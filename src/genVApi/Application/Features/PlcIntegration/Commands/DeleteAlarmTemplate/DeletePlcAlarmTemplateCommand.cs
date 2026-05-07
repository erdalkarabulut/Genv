using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Security.Constants;

namespace Application.Features.PlcIntegration.Commands.DeleteAlarmTemplate;

public class DeletePlcAlarmTemplateCommand : IRequest<DeletedPlcAlarmTemplateResponse>, ISecuredRequest, ILoggableRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [GeneralOperationClaims.Admin];

    public class Handler : IRequestHandler<DeletePlcAlarmTemplateCommand, DeletedPlcAlarmTemplateResponse>
    {
        private readonly IPlcAlarmTemplateRepository _repository;

        public Handler(IPlcAlarmTemplateRepository repository) => _repository = repository;

        public async Task<DeletedPlcAlarmTemplateResponse> Handle(DeletePlcAlarmTemplateCommand request, CancellationToken cancellationToken)
        {
            PlcAlarmTemplate? entity = await _repository.GetAsync(t => t.Id == request.Id, cancellationToken: cancellationToken);
            if (entity is null)
                throw new BusinessException("Alarm template bulunamadı.");

            await _repository.DeleteAsync(entity!);
            return new DeletedPlcAlarmTemplateResponse();
        }
    }
}

public sealed class DeletedPlcAlarmTemplateResponse
{
}
