using System.Collections.Generic;

namespace WsRaisedHandsModern.Api.DTOs
{
    public class CsvValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> HeaderErrors { get; set; } = new();
        public List<CertificateProcessingError> DataErrors { get; set; } = new();
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows => TotalRows - ValidRows;
    }
}