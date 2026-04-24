namespace Application.Services.SmsService;

public sealed record SmsSendResult(bool Success, string? ProviderReference, string? ErrorMessage);
