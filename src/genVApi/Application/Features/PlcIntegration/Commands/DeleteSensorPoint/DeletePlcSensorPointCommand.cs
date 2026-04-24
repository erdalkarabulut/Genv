using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Security.Constants;

namespace Application.Features.PlcIntegration.Commands.DeleteSensorPoint;

public class DeletePlcSensorPointCommand : IRequest<DeletedPlcSensorPointResponse>, ISecuredRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [GeneralOperationClaims.Admin];

    public class Handler : IRequestHandler<DeletePlcSensorPointCommand, DeletedPlcSensorPointResponse>
    {
        private readonly IPlcSensorPointRepository _repository;

        public Handler(IPlcSensorPointRepository repository) => _repository = repository;

        public async Task<DeletedPlcSensorPointResponse> Handle(DeletePlcSensorPointCommand request, CancellationToken cancellationToken)
        {
            PlcSensorPoint? entity = await _repository.GetAsync(p => p.Id == request.Id, cancellationToken: cancellationToken);
            if (entity is null)
                throw new BusinessException("Sensör kaydı bulunamadı.");

            await _repository.DeleteAsync(entity);

            return new DeletedPlcSensorPointResponse { Id = request.Id };
        }
    }
}
