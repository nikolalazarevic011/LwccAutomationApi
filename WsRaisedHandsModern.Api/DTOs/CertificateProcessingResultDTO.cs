using System;
using System.Collections.Generic;

namespace WsRaisedHandsModern.Api.DTOs
{
    public class CertificateProcessingResult
    {
        public int TotalRecords { get; set; }
        public int SuccessfullyProcessed { get; set; }
        public int Failed { get; set; }
        public List<CertificateProcessingError> Errors { get; set; } = new();
        public DateTime ProcessingStartTime { get; set; }
        public DateTime ProcessingEndTime { get; set; }
        public TimeSpan TotalProcessingTime => ProcessingEndTime - ProcessingStartTime;

        public bool HasErrors => Errors.Count > 0;
        public double SuccessRate => TotalRecords > 0 ? (double)SuccessfullyProcessed / TotalRecords * 100 : 0;
    }

    public class CertificateProcessingError
    {
        public int RowNumber { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorType { get; set; } = string.Empty; // "ValidationError", "EmailError", "CertificateGenerationError"
    }
}