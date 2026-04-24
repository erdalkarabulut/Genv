using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Security.Constants;

namespace Application.Features.PlcIntegration.Commands.DeleteAlarmContact;

public class DeletePlcAlarmContactCommand : IRequest<DeletedPlcAlarmContactResponse>, ISecuredRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [GeneralOperationClaims.Admin];

    public class Handler : IRequestHandler<DeletePlcAlarmContactCommand, DeletedPlcAlarmContactResponse>
    {
        private readonly IPlcAlarmContactRepository _repository;

        public Handler(IPlcAlarmContactRepository repository) => _repository = repository;

        public async Task<DeletedPlcAlarmContactResponse> Handle(DeletePlcAlarmContactCommand request, CancellationToken cancellationToken)
        {
            PlcAlarmContact? entity = await _repository.GetAsync(c => c.Id == request.Id, cancellationToken: cancellationToken);
            if (entity is null)
                throw new BusinessException("Bildirim kaydı bulunamadı.");

            await _repository.DeleteAsync(entity);

            return new DeletedPlcAlarmContactResponse { Id = request.Id };
        }
    }
}
