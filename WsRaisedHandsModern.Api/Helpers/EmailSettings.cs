using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WsRaisedHandsModern.Api.Helpers
{
    public class EmailSettings
    {
        public required string From { get; set; }
        public required string To { get; set; }
        public required string CC { get; set; }
        public required string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public required string SenderName { get; set; }
        public required string SenderEmail { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string OutputDirectory { get; set; }
    }
}