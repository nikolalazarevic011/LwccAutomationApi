using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WsRaisedHandsModern.Api.ViewModels
{
    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Username")]
        [StringLength(256, ErrorMessage = "Username cannot be longer than 256 characters.")]
        public string UserName { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Phone Number Confirmed")]
        public bool PhoneNumberConfirmed { get; set; }

        [Display(Name = "Two Factor Enabled")]
        public bool TwoFactorEnabled { get; set; }

        [Display(Name = "Lockout Enabled")]
        public bool LockoutEnabled { get; set; }

        [Display(Name = "Lockout End")]
        public DateTimeOffset? LockoutEnd { get; set; }

        [Display(Name = "Access Failed Count")]
        [Range(0, int.MaxValue, ErrorMessage = "Access failed count must be a non-negative number.")]
        public int AccessFailedCount { get; set; }

        [Display(Name = "New Password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        // Role management
        public IList<string> UserRoles { get; set; } = new List<string>();
        public List<string> AvailableRoles { get; set; } = new List<string>();
        public List<string> SelectedRoles { get; set; } = new List<string>();

    }
}