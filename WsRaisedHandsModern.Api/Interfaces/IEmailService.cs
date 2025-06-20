using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WsRaisedHandsModern.Api.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message, IEnumerable<System.Net.Mail.Attachment>? attachments = null);
        // Task<bool> SendEmailResetPassword(string toEmail, string fromEmail, string fromPassword, string smtpHost, string userName, string resetLink);
    }
}