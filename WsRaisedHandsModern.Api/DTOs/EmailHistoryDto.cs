using System;
using System.ComponentModel.DataAnnotations;

namespace WsRaisedHandsModern.Api.DTOs
{
    public class EmailHistoryDto
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string ToEmail { get; set; } = string.Empty;
        public string? CcEmail { get; set; }
        public string? BccEmail { get; set; }
        public DateTime DateSent { get; set; }
        public string EmailType { get; set; } = string.Empty;
        public string? ReportDateRange { get; set; }
        public int? RecordCount { get; set; }
        public string? AttachmentFilenames { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string SentBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class EmailHistoryQueryDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? EmailType { get; set; }
        public string? Status { get; set; }
        [Range(1, 1000)]
        public int Limit { get; set; } = 50;
    }
}