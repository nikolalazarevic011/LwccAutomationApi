using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WsRaisedHandsModern.Api.Data.AppData.Entities
{
    public class AppUser : IdentityUser<int>  
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime LastActive { get; set; } = DateTime.UtcNow;
        //add other properties as needed
        public ICollection<AppUserRole> UserRoles { get; set; }
    }
}