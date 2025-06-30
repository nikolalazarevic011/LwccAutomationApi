using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WsRaisedHandsModern.Api.Data.AppData;
using WsRaisedHandsModern.Api.Data.AppData.Entities;

namespace WsRaisedHandsModern.Api.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            // builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
              // Use AddDefaultIdentity instead of AddIdentity for UI compatibility
            services.AddDefaultIdentity<AppUser>(options =>
            {
                // Password requirements (make sure seed password meets these)
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;

                // Account lockout settings
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                options.Lockout.MaxFailedAccessAttempts = 50;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false; // Important for testing!
                options.SignIn.RequireConfirmedAccount = false; // Important for testing!
                
                // Two-Factor Authentication settings - ADD THESE
                options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
                options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                
                // Store settings
                options.Stores.MaxLengthForKeys = 128;
                options.Stores.ProtectPersonalData = false;
            })
            .AddRoles<AppRole>() // Add this to support roles
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

              //add lifetime of token
            services.Configure<DataProtectionTokenProviderOptions>(o =>
                o.TokenLifespan = TimeSpan.FromHours(2));

              // Configure JWT authentication
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));

            var isDevelopment = config.GetValue<bool>("IsDevelopment");

            services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = false, // API Server
                        ValidateAudience = false, // Angular Application

                            //uncomment for production use
                        /*ValidateIssuer = true,
                            ValidIssuer = config["TokenIssuer"], // e.g. "myapp.com"
                            ValidateAudience = true,
                            ValidAudience = config["TokenAudience"], // e.g. "myapp_client"
                            */
                        /*
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = key,
                            ValidateIssuer = !isDevelopment,
                            ValidIssuer = isDevelopment ? null : config["TokenIssuer"],
                            ValidateAudience = !isDevelopment,
                            ValidAudience = isDevelopment ? null : config["TokenAudience"],
                            ClockSkew = TimeSpan.FromMinutes(5) // Account for clock drift
                            */
                    };
                });

              // Configure authorization policies with explicit scheme
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy =>
                {
                    policy.RequireRole("Admin");
                    policy.AddAuthenticationSchemes("Identity.Application", "Bearer");
                });
                options.AddPolicy("ModeratePhotoRole", policy =>
                {
                    policy.RequireRole("Moderator");
                    policy.AddAuthenticationSchemes("Identity.Application", "Bearer");
                });
                options.AddPolicy("ModerateFilesRole", policy =>
                {
                    policy.RequireRole("Editor", "Admin");
                    policy.AddAuthenticationSchemes("Identity.Application", "Bearer");
                });
                options.AddPolicy("RequireModeratorOrHigher", policy => 
                {
                    policy.RequireRole("Moderator", "Admin");
                    policy.AddAuthenticationSchemes("Identity.Application", "Bearer");
                });
                
                options.AddPolicy("RequireStaffAccess", policy => 
                {
                    policy.RequireRole("Staff", "Faculty", "Admin");
                    policy.AddAuthenticationSchemes("Identity.Application", "Bearer");
                });
            });

            return services;
        }
    }
}