using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WsRaisedHandsModern.Api.ViewModels
{
    public class CreateUserViewModel
    {
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

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; } = true;

        [Display(Name = "Phone Number Confirmed")]
        public bool PhoneNumberConfirmed { get; set; } = false;

        [Display(Name = "Two Factor Enabled")]
        public bool TwoFactorEnabled { get; set; } = false;

        [Display(Name = "Lockout Enabled")]
        public bool LockoutEnabled { get; set; } = false;

        [Display(Name = "Send Welcome Email")]
        public bool SendWelcomeEmail { get; set; } = true;

        // Role management
        public List<string> AvailableRoles { get; set; } = new List<string>();
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }
}