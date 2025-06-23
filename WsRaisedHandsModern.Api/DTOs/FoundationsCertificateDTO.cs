using System;
using System.ComponentModel.DataAnnotations;

namespace WsRaisedHandsModern.Api.DTOs
{
    public class FoundationsCertificateDTO
    {
        [Required, EmailAddress, MaxLength(255)]
        public required string Email { get; set; }

        [Required, MaxLength(50)]
        public required string FirstName { get; set; }

        [Required, MaxLength(50)]
        public required string LastName { get; set; }

        [Required]
        public required DateTime CompletionDate { get; set; }

        // Computed property for full name
        public string FullName => $"{FirstName} {LastName}";

        // Computed property for formatted date
        public string FormattedCompletionDate => 
            $"on the {CompletionDate.Day}{GetOrdinalSuffix(CompletionDate.Day)} day of {CompletionDate:MMMM}, {CompletionDate.Year}";

        private static string GetOrdinalSuffix(int day)
        {
            if (day >= 11 && day <= 13)
                return "th";

            return (day % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
        }
    }
}