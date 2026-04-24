using Application.Services.SmsService;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Security.Constants;

namespace Application.Features.Sms.Commands.SendTestSms;

public class SendTestSmsCommand : IRequest<SendTestSmsResponse>, ISecuredRequest, ILoggableRequest
{
    public string PhoneNumber { get; set; } = "";
    public string Message { get; set; } = "";

    public string[] Roles => [GeneralOperationClaims.Admin];

    public class Handler : IRequestHandler<SendTestSmsCommand, SendTestSmsResponse>
    {
        private readonly ISmsSender _sms;

        public Handler(ISmsSender sms) => _sms = sms;

        public async Task<SendTestSmsResponse> Handle(SendTestSmsCommand request, CancellationToken cancellationToken)
        {
            SmsSendResult r = await _sms.SendAsync(request.PhoneNumber, request.Message, cancellationToken);
            return new SendTestSmsResponse
            {
                Success = r.Success,
                ProviderReference = r.ProviderReference,
                ErrorMessage = r.ErrorMessage,
            };
        }
    }
}

public sealed class SendTestSmsResponse
{
    public bool Success { get; set; }
    public string? ProviderReference { get; set; }
    public string? ErrorMessage { get; set; }
}
