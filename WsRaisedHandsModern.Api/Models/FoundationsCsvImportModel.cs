using System;
using System.ComponentModel.DataAnnotations;

namespace WsRaisedHandsModern.Api.Models
{
    public class FoundationsCsvImportModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string CompletionDate { get; set; } = string.Empty;

        // Validation method to check if all required fields are present
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(CompletionDate);
        }

        // Method to parse completion date
        public DateTime? GetParsedCompletionDate()
        {
            if (DateTime.TryParse(CompletionDate, out DateTime result))
            {
                return result;
            }
            return null;
        }

        // Method to validate email format
        public bool IsValidEmail()
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(Email);
                return addr.Address == Email;
            }
            catch
            {
                return false;
            }
        }
    }
}