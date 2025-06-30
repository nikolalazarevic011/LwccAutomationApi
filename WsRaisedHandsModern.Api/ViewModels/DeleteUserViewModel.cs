using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WsRaisedHandsModern.Api.ViewModels
{
    public class DeleteUserViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }
        
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }
        
        public string? Email { get; set; }
        
        [Display(Name = "Username")]
        public string? UserName { get; set; }
        
        public DateTime Created { get; set; }
        
        [Display(Name = "Last Active")]
        public DateTime LastActive { get; set; }
        
        [Display(Name = "User Roles")]
        public IList<string> UserRoles { get; set; } = new List<string>();
        
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}