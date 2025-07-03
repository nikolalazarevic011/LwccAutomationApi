using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WsRaisedHandsModern.Api.Data.AppData.Entities
{
    [Table("EmailHistory")]
    public class EmailHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string ToEmail { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? CcEmail { get; set; }

        [MaxLength(500)]
        public string? BccEmail { get; set; }

        [Required]
        public DateTime DateSent { get; set; }

        [Required]
        [MaxLength(50)]
        public string EmailType { get; set; } = string.Empty; // e.g., "RaisedHandsSalvation", "WeeklyReport", etc.

        [MaxLength(100)]
        public string? ReportDateRange { get; set; } // e.g., "2025-06-01 to 2025-06-30"

        public int? RecordCount { get; set; } // Number of records in the report

        [MaxLength(1000)]
        public string? AttachmentFilenames { get; set; } // Comma-separated list of attachment filenames

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Sent"; // Sent, Failed, etc.

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        [Required]
        [MaxLength(100)]
        public string SentBy { get; set; } = "System"; // User who triggered the email or "System"

        public DateTime CreatedAt { get; set; } // Let SQL Server handle the default via GETUTCDATE()
    }
}