using amorphie.token.core.Enums.MessagingGateway;

namespace amorphie.token.core.Models.MessagingGateway;

public class PushRequest
{
    public SenderType Sender { get; set; }
    public string Template { get; set; }
    public string TemplateParams { get; set; }
    public long? CustomerNo { get; set; }
    public string? CitizenshipNo { get; set; }
    public string? NotificationType { get; set; }
    public Process Process { get; set; }
}
