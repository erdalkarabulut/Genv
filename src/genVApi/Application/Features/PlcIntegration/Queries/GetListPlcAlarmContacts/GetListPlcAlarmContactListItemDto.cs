namespace Application.Features.PlcIntegration.Queries.GetListPlcAlarmContacts;

public sealed class GetListPlcAlarmContactListItemDto
{
    public Guid Id { get; set; }
    public string? DevicePrefix { get; set; }
    public Guid? AlarmTemplateId { get; set; }
    public string? AlarmTemplateName { get; set; }
    public string DisplayName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public bool SmsEnabled { get; set; }
    public bool EmailEnabled { get; set; }
}
