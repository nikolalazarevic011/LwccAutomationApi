using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WsRaisedHandsModern.Api.Data.AppData.Entities;
using WsRaisedHandsModern.Api.ViewModels;

namespace WsRaisedHandsModern.Api.Controllers
{
    [Authorize(Policy = "RequireAdminRole")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
                UserManager<AppUser> userManager,
                RoleManager<AppRole> roleManager,
                ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading admin user index page");

                var users = _userManager.Users.ToList();

                _logger.LogInformation("Retrieved {UserCount} users for admin view", users.Count);

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin user index");
                return View(new List<AppUser>()); // Return empty list instead of null
            }
        }

        public IActionResult TestPage()
        {
            return View("TestPage");
        }

        public IActionResult Reports()
        {
            return View("Reports");
        }


        public async Task<IActionResult> Users()
        {
            try
            {
                _logger.LogInformation("Loading admin user index page");

                var users = _userManager.Users.ToList();

                _logger.LogInformation("Retrieved {UserCount} users for admin view", users.Count);

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin user index");
                return View(new List<AppUser>()); // Return empty list instead of null
            }
        }


        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }
            return View("UserDetails", user);
        }

          // GET: Admin/Create
        public IActionResult Create()
        {
            var model = new CreateUserViewModel
            {
                AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList(),
                EmailConfirmed = true, // Default to confirmed for admin-created users
                SendWelcomeEmail = true
            };
        
            return View(model);
        }
        
        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new AppUser
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        UserName = model.UserName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        EmailConfirmed = model.EmailConfirmed,
                        PhoneNumberConfirmed = model.PhoneNumberConfirmed,
                        TwoFactorEnabled = model.TwoFactorEnabled,
                        LockoutEnabled = model.LockoutEnabled,
                        Created = DateTime.UtcNow,
                        LastActive = DateTime.UtcNow,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };
        
                    var result = await _userManager.CreateAsync(user, model.Password);
        
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User {UserName} created successfully by admin", user.UserName);
        
                        // Add selected roles
                        if (model.SelectedRoles != null && model.SelectedRoles.Any())
                        {
                            var roleResult = await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                            if (!roleResult.Succeeded)
                            {
                                _logger.LogWarning("Failed to add roles to user {UserName}: {Errors}",
                                    user.UserName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                                
                                // Still consider creation successful, just log the role issue
                                foreach (var error in roleResult.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, $"Role assignment error: {error.Description}");
                                }
                            }
                            else
                            {
                                _logger.LogInformation("Added roles {Roles} to user {UserName}",
                                    string.Join(", ", model.SelectedRoles), user.UserName);
                            }
                        }
                        else
                        {
                            // Add default Member role if no roles selected
                            if (await _roleManager.RoleExistsAsync("Member"))
                            {
                                await _userManager.AddToRoleAsync(user, "Member");
                                _logger.LogInformation("Added default Member role to user {UserName}", user.UserName);
                            }
                        }
        
                        // Send welcome email if requested
                        if (model.SendWelcomeEmail)
                        {
                            try
                            {
                                // TODO: Implement email service
                                // await _emailService.SendWelcomeEmailAsync(user, model.Password);
                                _logger.LogInformation("Welcome email would be sent to {Email}", user.Email);
                            }
                            catch (Exception emailEx)
                            {
                                _logger.LogError(emailEx, "Failed to send welcome email to {Email}", user.Email);
                                // Don't fail user creation if email fails
                            }
                        }
        
                        TempData["SuccessMessage"] = $"User '{user.Email}' has been created successfully.";
                        return RedirectToAction(nameof(Details), new { id = user.Id });
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        _logger.LogWarning("Failed to create user {UserName}: {Errors}",
                            model.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating user {UserName}", model.UserName);
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the user.");
                }
            }
        
            // If we got this far, something failed, redisplay form
            model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }


        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                UserRoles = await _userManager.GetRolesAsync(user),
                AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList()
            };

            return View(model);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(id.ToString());
                    if (user == null)
                    {
                        return NotFound();
                    }

                    // Update basic properties
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.EmailConfirmed = model.EmailConfirmed;
                    user.PhoneNumberConfirmed = model.PhoneNumberConfirmed;
                    user.TwoFactorEnabled = model.TwoFactorEnabled;
                    user.LockoutEnabled = model.LockoutEnabled;
                    user.AccessFailedCount = model.AccessFailedCount;

                    // Handle lockout end
                    if (model.LockoutEnd.HasValue)
                    {
                        user.LockoutEnd = model.LockoutEnd.Value;
                    }
                    else
                    {
                        user.LockoutEnd = null;
                    }

                    // Update email if changed
                    if (user.Email != model.Email)
                    {
                        var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                        if (!setEmailResult.Succeeded)
                        {
                            foreach (var error in setEmailResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            model.UserRoles = await _userManager.GetRolesAsync(user);
                            model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                            return View(model);
                        }
                    }

                    // Update username if changed
                    if (user.UserName != model.UserName)
                    {
                        var setUsernameResult = await _userManager.SetUserNameAsync(user, model.UserName);
                        if (!setUsernameResult.Succeeded)
                        {
                            foreach (var error in setUsernameResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            model.UserRoles = await _userManager.GetRolesAsync(user);
                            model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                            return View(model);
                        }
                    }

                    // Update other properties
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        foreach (var error in updateResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        model.UserRoles = await _userManager.GetRolesAsync(user);
                        model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                        return View(model);
                    }

                    // Handle role changes
                    if (model.SelectedRoles != null)
                    {
                        var currentRoles = await _userManager.GetRolesAsync(user);
                        var rolesToRemove = currentRoles.Except(model.SelectedRoles).ToList();
                        var rolesToAdd = model.SelectedRoles.Except(currentRoles).ToList();

                        if (rolesToRemove.Any())
                        {
                            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                            if (!removeResult.Succeeded)
                            {
                                foreach (var error in removeResult.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                                model.UserRoles = await _userManager.GetRolesAsync(user);
                                model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                                return View(model);
                            }
                        }

                        if (rolesToAdd.Any())
                        {
                            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                            if (!addResult.Succeeded)
                            {
                                foreach (var error in addResult.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                                model.UserRoles = await _userManager.GetRolesAsync(user);
                                model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                                return View(model);
                            }
                        }
                    }

                    // Handle password reset if provided
                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var resetResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                        if (!resetResult.Succeeded)
                        {
                            foreach (var error in resetResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            model.UserRoles = await _userManager.GetRolesAsync(user);
                            model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                            return View(model);
                        }
                    }

                    _logger.LogInformation("User {UserId} updated successfully by admin", user.Id);
                    TempData["SuccessMessage"] = "User updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = user.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user {UserId}", id);
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the user.");
                }
            }

            // If we got this far, something failed, redisplay form
            model.UserRoles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(id.ToString()));
            model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

         // GET: Admin/Delete/5 - Show confirmation page
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }
        
            var model = new DeleteUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                Created = user.Created,
                LastActive = user.LastActive,
                UserRoles = await _userManager.GetRolesAsync(user)
            };
        
            return View(model);
        }
        
        // POST: Admin/Delete/5 - Actually delete the user
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return NotFound();
                }
        
                // Log the deletion attempt
                _logger.LogWarning("Admin attempting to delete user {UserId} ({Email})", user.Id, user.Email);
        
                // Remove user from all roles first
                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Any())
                {
                    var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, userRoles);
                    if (!removeRolesResult.Succeeded)
                    {
                        _logger.LogError("Failed to remove roles from user {UserId}: {Errors}", 
                            user.Id, string.Join(", ", removeRolesResult.Errors.Select(e => e.Description)));
                        
                        TempData["ErrorMessage"] = "Failed to remove user roles. User was not deleted.";
                        return RedirectToAction(nameof(Delete), new { id });
                    }
                }
        
                // Delete the user
                var deleteResult = await _userManager.DeleteAsync(user);
                if (deleteResult.Succeeded)
                {
                    _logger.LogWarning("User {UserId} ({Email}) has been deleted by admin", user.Id, user.Email);
                    TempData["SuccessMessage"] = $"User '{user.Email}' has been successfully deleted.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogError("Failed to delete user {UserId}: {Errors}", 
                        user.Id, string.Join(", ", deleteResult.Errors.Select(e => e.Description)));
                    
                    var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
                    TempData["ErrorMessage"] = $"Failed to delete user: {errors}";
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the user.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }
        
        // AJAX endpoint for immediate deletion (optional)
        [HttpPost]
        public async Task<IActionResult> DeleteUserAjax(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }
        
                // Remove from roles first
                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Any())
                {
                    var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, userRoles);
                    if (!removeRolesResult.Succeeded)
                    {
                        return Json(new { success = false, message = "Failed to remove user roles." });
                    }
                }
        
                // Delete the user
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogWarning("User {UserId} ({Email}) deleted via AJAX by admin", user.Id, user.Email);
                    return Json(new { success = true, message = "User deleted successfully." });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, message = $"Failed to delete user: {errors}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AJAX delete for user {UserId}", id);
                return Json(new { success = false, message = "An unexpected error occurred." });
            }
        }
        

    }
}