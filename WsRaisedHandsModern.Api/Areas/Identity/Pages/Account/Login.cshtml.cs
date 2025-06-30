// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WsRaisedHandsModern.Api.Data.AppData.Entities;

namespace WsRaisedHandsModern.Api.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;  //added for user verification
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<AppUser> signInManager, ILogger<LoginModel> logger, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;  //added for user verification
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            /*[Required]
            [EmailAddress]
            public string Email { get; set; }*/
            [Required]
            [Display(Name = "Email or Username")]
            public string EmailOrUsername { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Login attempt for: {EmailOrUsername}", Input.EmailOrUsername);
        
                // Determine if input is email or username
                var isEmail = IsValidEmail(Input.EmailOrUsername);
                AppUser user = null;
        
                if (isEmail)
                {
                    user = await _userManager.FindByEmailAsync(Input.EmailOrUsername);
                    _logger.LogInformation("Attempting login with email: {Email}", Input.EmailOrUsername);
                }
                else
                {
                    user = await _userManager.FindByNameAsync(Input.EmailOrUsername);
                    _logger.LogInformation("Attempting login with username: {Username}", Input.EmailOrUsername);
                }
        
                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found for input: {EmailOrUsername}", Input.EmailOrUsername);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
        
                // Validate password
                var passwordValid = await _userManager.CheckPasswordAsync(user, Input.Password);
                if (!passwordValid)
                {
                    _logger.LogWarning("Login failed: Invalid password for user: {EmailOrUsername}", Input.EmailOrUsername);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
        
                // Log user details for debugging
                _logger.LogInformation("User found: UserName={UserName}, Email={Email}, 2FA={TwoFactorEnabled}", 
                    user.UserName, user.Email, user.TwoFactorEnabled);
        
                // Disable 2FA if enabled (for testing)
                if (user.TwoFactorEnabled)
                {
                    _logger.LogInformation("Disabling 2FA for user {EmailOrUsername}", Input.EmailOrUsername);
                    await _userManager.SetTwoFactorEnabledAsync(user, false);
                }
        
                // Try to sign in - use the user's actual username for sign-in
                var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        
                _logger.LogInformation("Login result for {EmailOrUsername}: Succeeded={Succeeded}, IsLockedOut={IsLockedOut}, RequiresTwoFactor={RequiresTwoFactor}, IsNotAllowed={IsNotAllowed}",
                    Input.EmailOrUsername, result.Succeeded, result.IsLockedOut, result.RequiresTwoFactor, result.IsNotAllowed);
        
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {EmailOrUsername} logged in successfully", Input.EmailOrUsername);
                    
                    // Update last active time
                    user.LastActive = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                    
                    return LocalRedirect(returnUrl);
                }
        
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
        
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out: {EmailOrUsername}", Input.EmailOrUsername);
                    return RedirectToPage("./Lockout");
                }
        
                if (result.IsNotAllowed)
                {
                    _logger.LogWarning("User {EmailOrUsername} is not allowed to sign in", Input.EmailOrUsername);
                    ModelState.AddModelError(string.Empty, "Account is not confirmed.");
                    return Page();
                }
        
                _logger.LogWarning("Login failed for {EmailOrUsername} - unknown reason", Input.EmailOrUsername);
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
        
            return Page();
        }

        // Helper method to validate email format
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
        
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        
        /*public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true

                // Find the user first to check their status
                // var email = Input.Email?.Trim().ToLowerInvariant();
                // var password = Input.Password?.Trim();
                string email = Input.Email;
                string password = Input.Password;

                var user = await _userManager.FindByEmailAsync(email);
                // var user = await _userManager.FindByEmailAsync(Input.Email); //added for user verification
                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found for email {Email}", Input.Email);
                    ModelState.AddModelError(string.Empty, "User not found.");  
                    return Page();
                }
                   // Check password validity - debugging purposes
                var passwordValid = await _userManager.CheckPasswordAsync(user, password);
                if (!passwordValid)
                {
                    _logger.LogWarning("Login failed: Password Incorrect for email {Email}", Input.Email);
                    ModelState.AddModelError(string.Empty, "Invalid Password.");  
                    return Page();
                }

                  // Check user status - debugging purposes
                    //var canSignIn = await _signInManager.CanSignInAsync(user);
                    //_logger.LogInformation("Can sign in: {CanSignIn}", canSignIn);

                    // Check if user requires 2FA - debugging purposes
                    //var requiresTwoFactor = await _userManager.GetTwoFactorEnabledAsync(user);

                    // Check user roles - debugging purposes
                    //var roles = await _userManager.GetRolesAsync(user);

                    // Check authentication schemes - debugging purposes
                    //var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();

                    // Check if 2FA is enabled for this user
                    _logger.LogInformation("User {Email} - 2FA Enabled: {TwoFactorEnabled}", Input.Email, user.TwoFactorEnabled);

                // Disable 2FA if it's enabled (for testing purposes)
                if (user.TwoFactorEnabled)
                {
                    _logger.LogInformation("Disabling 2FA for user {Email}", Input.Email);
                    user.TwoFactorEnabled = false;
                    await _userManager.UpdateAsync(user);
                }

                  //Origional login logic
                  //var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
  
                  // Attempt login with email, problems with 2FA hardcoded to true in static props
                  //var signInResultEmail = await _signInManager.PasswordSignInAsync(email, password, Input.RememberMe, lockoutOnFailure: false);

                  // Try signing in with USERNAME
                var result = await _signInManager.PasswordSignInAsync(user.UserName, password, false, lockoutOnFailure: false);

                _logger.LogInformation("Login result for {Email}: Succeeded={Succeeded}, IsLockedOut={IsLockedOut}, RequiresTwoFactor={RequiresTwoFactor}, IsNotAllowed={IsNotAllowed}",
                    Input.Email, result.Succeeded, result.IsLockedOut, result.RequiresTwoFactor, result.IsNotAllowed);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in successfully", Input.Email);

                    // Update last active time
                    user.LastActive = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                    return LocalRedirect(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }*/
    }
    public class LoginMdl
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
