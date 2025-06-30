using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WsRaisedHandsModern.Api.Data.AppData.Entities;
using WsRaisedHandsModern.Api.Data.AppData.Interfaces;

namespace WsRaisedHandsModern.Api.Data.AppData
{
    public class Seed
    {
        public static async Task SeedUsers(IServiceProvider serviceProvider)
        {

            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
            var logger = serviceProvider.GetRequiredService<ILogger<Seed>>();
            // var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

            // 1. Seed Identity tables (no UnitOfWork needed)
            await SeedIdentityData(userManager, roleManager, logger);
            

            // 2. Seed custom tables (use UnitOfWork)
            //  await SeedCustomTables(unitOfWork, logger);

        }

        private static async Task SeedIdentityData(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ILogger<Seed> logger)
        {

            logger.LogInformation("Starting Identity data seeding...");

            // Define Roles
            var roleNames = new[] { "Subscriber", "Member", "Contributor", "Author", "Editor", "Moderator", "Faculty", "Staff", "Admin" };

            // Create Roles
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await roleManager.CreateAsync(new AppRole { Name = roleName });
                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation("Created role: {RoleName}", roleName);
                    }
                    else
                    {
                        logger.LogError("Failed to create role {RoleName}: {Errors}", roleName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
            }

            // Read and process user data
            if (File.Exists("UserSeedData.json"))
            {
                var userData = await File.ReadAllTextAsync("UserSeedData.json");
                var users = JsonSerializer.Deserialize<List<AppUser>>(userData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                foreach (var user in users)
                {
                    //Create Users if they dont exist
                    if (await userManager.FindByNameAsync(user.UserName) == null)
                    {
                        user.UserName = user.UserName.ToLower();
                        user.Created = DateTime.SpecifyKind(user.Created, DateTimeKind.Utc);
                        user.LastActive = DateTime.SpecifyKind(user.LastActive, DateTimeKind.Utc);

                            // Add the security and confirmation fields for all seeded users
                        user.EmailConfirmed = true;
                        user.PhoneNumberConfirmed = true;
                        user.TwoFactorEnabled = false;        // Explicitly disable 2FA
                        user.LockoutEnabled = false;          // Disable lockout
                        user.AccessFailedCount = 0;           // Reset failed attempts
                        user.LockoutEnd = null;               // Ensure no lockout end date
                        //user.SecurityStamp = Guid.NewGuid().ToString(); // Generate security stamp


                        var createResult = await userManager.CreateAsync(user, "Pa$$w0rd");
                        if (createResult.Succeeded)
                        {
                            var roleResult = await userManager.AddToRoleAsync(user, "Member");
                            if (!roleResult.Succeeded)
                            {
                                logger.LogError("Failed to add user {UserName} to Member role: {Errors}", user.UserName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                            }
                        }
                        else
                        {
                            logger.LogError("Failed to create user {UserName}: {Errors}", user.UserName, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                        }
                    }
                }
            }

            // Create admin user
            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                var admin = new AppUser
                {
                    UserName = "admin",
                    Email = "admin@yourapp.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true, // Add this
                    TwoFactorEnabled = false, // Explicitly disable 2FA
                    LockoutEnabled = false,      // Add this
                    AccessFailedCount = 0,       // Add this
                    Created = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(admin, "Pa$$w0rd");
                if (createResult.Succeeded)
                {
                    var roleResult = await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator", "Member" });
                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation("Added admin to roles successfully");
                    }
                    else
                    {
                        logger.LogError("Failed to add admin to roles: {Errors}", string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("Admin user already exists");
            }
        
            logger.LogInformation("Identity data seeding completed");
        }



        /* Example of how to seed custom tables after users have been created */
/*
        private static async Task SeedCustomTables(IUnitOfWork unitOfWork, ILogger logger)
        {
            try
            {
                // Example: Seed Categories
                if (!unitOfWork.CategoryRepository.GetAll().Any())
                {
                    var categories = new List<Category>
                    {
                        new Category { Name = "Technology", Description = "Tech related posts" },
                        new Category { Name = "Education", Description = "Educational content" },
                        new Category { Name = "News", Description = "News and updates" }
                    };

                    foreach (var category in categories)
                    {
                        unitOfWork.CategoryRepository.Add(category);
                    }
                }

                // Example: Seed Posts (after users exist)
                if (!unitOfWork.PostRepository.GetAll().Any())
                {
                    var adminUser = await unitOfWork.UserRepository.GetByUsernameAsync("admin");
                    if (adminUser != null)
                    {
                        var posts = new List<Post>
                        {
                            new Post
                            {
                                Title = "Welcome Post",
                                Content = "Welcome to our platform!",
                                AuthorId = adminUser.Id,
                                CreatedDate = DateTime.UtcNow
                            }
                        };

                        foreach (var post in posts)
                        {
                            unitOfWork.PostRepository.Add(post);
                        }
                    }
                }

                // Save all custom table changes
                await unitOfWork.Complete();
                logger.LogInformation("Custom tables seeded successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error seeding custom tables");
                throw;
            }
           
        } */

        }
    
}