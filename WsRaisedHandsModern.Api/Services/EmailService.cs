using System;
using System.Threading.Tasks;
using WsRaisedHandsModern.Api.Interfaces;
using WsRaisedHandsModern.Api.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;
using System.Collections.Generic;

namespace WsRaisedHandsModern.Api.Services
{
    public class EmailService : IEmailService
    {
        //private readonly IConfiguration _config;
        //from : not used
        //SmtpServer: necessary for correct email sending
        //SmtpPort: necessary for correct email sending, default is 587
        //Username: necessary for correct email sending, this is the email address
        //Password: necessary for correct email sending, this is the email password
        //SenderName: necessary for correct email sending, this is the name of the sender for display purposes, can be changed
        //SenderEmail: necessary for correct email sending, this is the email address of the sender for display purposes, can be changed
        //OutputDirectory: used for testing to output the email to a file
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        

        // private readonly Serilog.Sinks.File.FileSink _log;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message, IEnumerable<Attachment>? attachments = null)
        {
              // _logger.LogDebug("Preparing to send email to {ToEmail} with subject {Subject}", toEmail, subject);
            
            if (!string.IsNullOrEmpty(_emailSettings.OutputDirectory))
            {
                // Output email to file
                var outputPath = Path.Combine(_emailSettings.OutputDirectory, $"{Guid.NewGuid()}.eml");
                Directory.CreateDirectory(_emailSettings.OutputDirectory);
                await File.WriteAllTextAsync(outputPath, message);
                _logger.LogWarning("Email saved to {OutputPath}", outputPath);
            }
            else
            {
                  // Send email via SMTP
                using (var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);
                    smtp.Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password);
                    smtp.EnableSsl = true;

                    mailMessage.To.Add(toEmail);
                    mailMessage.Subject = subject;

                       // mailMessage.Body = HttpUtility.HtmlDecode(message); //message;
                    mailMessage.Body = message;
                    mailMessage.IsBodyHtml = true;

                    ContentType mimeType = new ContentType("text/html");
                    AlternateView alternate = AlternateView.CreateAlternateViewFromString(message, mimeType);
                    mailMessage.AlternateViews.Add(alternate);
                    
                    //email debugging code
                    // _logger.LogInformation("mail message: {mailMessage}", mailMessage);
                    // var credentials = smtp.Credentials.GetCredential(smtp.Host, smtp.Port, "Basic");
                    // _logger.LogInformation("SMTP Credentials: UserName={UserName}, Password={Password}", credentials.UserName, credentials.Password);
                    // _logger.LogInformation("smtp.Host: {smtp.Host}", smtp.Host);
                    // _logger.LogInformation("smtp.Port: {smtp.Port}", smtp.Port);
                    // _logger.LogInformation("smtp.Credentials: {smtp.Credentials}", smtp.Credentials);
                    //  _logger.LogDebug("SMTP Client Config: Host={Host}, Port={Port}, EnableSsl={EnableSsl}", _emailSettings.SmtpServer, _emailSettings.SmtpPort, smtp.EnableSsl);
                    // _logger.LogDebug("Email Message: From={From}, To={To}, Subject={Subject}", mailMessage.From, mailMessage.To, mailMessage.Subject);
                    
                                // Add attachments if any
                    if (attachments != null)
                    {
                        foreach (var attachment in attachments)
                        {
                            mailMessage.Attachments.Add(attachment);
                        }
                    }
            
                    try
                    {
                        await smtp.SendMailAsync(mailMessage);
                        // smtp.Send(mailMessage);
                        _logger.LogWarning("Email sent to {ToEmail} with subject {Subject}", toEmail, subject);
                    }
                    catch (SmtpException smtpEx)
                    {
                        _logger.LogError(smtpEx, "SMTP Error: {Message}, StatusCode: {StatusCode}", smtpEx.Message, smtpEx.StatusCode);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "General Error sending email to {ToEmail} with subject {Subject}", toEmail, subject);
                        throw;
                    }
                    
                }
            }
        }

    }
}