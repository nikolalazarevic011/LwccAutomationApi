using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WsRaisedHandsModern.Api.Data.CMSReporting.Entities
{
    public class tblFormSalvation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AutoNum { get; set; }

        [MaxLength(5)]
        public string? SavedFlag { get; set; }

        [Required, MaxLength(50)]
        public required string FirstName { get; set; }

        [Required, MaxLength(50)]
        public required string LastName { get; set; }

        [MaxLength(255)]
        public string? Country { get; set; }

        [MaxLength(255)]
        public string? Address1 { get; set; }

        [MaxLength(255)]
        public string? Address2 { get; set; }

        [MaxLength(255)]
        public string? City { get; set; }

        [MaxLength(255)]
        public string? StateRegionProvince { get; set; }

        [MaxLength(50)]
        public string? PostalCode { get; set; }

        [MaxLength(50)]
        public string? PhoneNumber { get; set; }

        [Required, MaxLength(255)]
        public required string Email { get; set; }

        [Required, MaxLength(5)]
        public required string ContactFlag { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        [MaxLength(50)]
        public string? Source { get; set; }

        [MaxLength(5)]
        public string? EnewsletterFlag { get; set; }

        [MaxLength(5)]
        public string? TextblastsFlag { get; set; }

        [MaxLength(5)]
        public string? NonMemberFlag { get; set; }
    }
}


