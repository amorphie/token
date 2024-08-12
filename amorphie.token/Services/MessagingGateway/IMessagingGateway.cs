using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amorphie.token.core.Models.MessagingGateway;
using Refit;

namespace amorphie.token.Services.MessagingGateway
{
    public interface IMessagingGateway
    {
        [Post("/api/v2/Messaging/sms/message/string")]
        Task<MessageResponse> SendSms(SmsRequest request);
        [Post("/api/v2/Messaging/push-notification/templated")]
        Task<MessageResponse> SendPush(PushRequest request);
    }
}
