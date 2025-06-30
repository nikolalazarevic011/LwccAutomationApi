using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WsRaisedHandsModern.Api.Data.AppData.Entities;

namespace WsRaisedHandsModern.Api.Data.AppData
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, int, 
            IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, 
            IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the relationships
            builder.Entity<AppUserRole>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                userRole.HasOne(ur => ur.User)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });
            
            builder.Entity<AppUser>().ToTable("AspNetUsers");
            builder.Entity<AppRole>().ToTable("AspNetRoles");
            builder.Entity<AppUserRole>().ToTable("AspNetUserRoles");

        }
    }
}