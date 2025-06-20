using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WsRaisedHandsModern.Api.Helpers
{
    public class EmailSettings
    {
        public string From { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string OutputDirectory { get; set; }
    }
}