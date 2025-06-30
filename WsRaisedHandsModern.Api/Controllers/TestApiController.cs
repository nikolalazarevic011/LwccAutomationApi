using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WsRaisedHandsModern.Api.Data.AppData.Entities;

namespace WsRaisedHandsModern.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestApiController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<TestApiController> _logger;

        public TestApiController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<TestApiController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }


        [HttpPost("verify-user/{username}")]
        public async Task<IActionResult> VerifyUser(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return NotFound($"User {username} not found");
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                Roles = roles,
                Created = user.Created,
                LastActive = user.LastActive
            });
        }

        [HttpPost("test-login")]
        public async Task<IActionResult> TestLogin([FromBody] LoginTestModel model)
        {
            _logger.LogInformation("Testing login for user: {Username}", model.Username);

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            // Check password
            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.Password);
            _logger.LogInformation("Password check result: {PasswordValid}", passwordCheck);

            if (!passwordCheck)
            {
                return BadRequest("Invalid password");
            }

            // Check if user can sign in
            var canSignIn = await _signInManager.CanSignInAsync(user);
            _logger.LogInformation("Can sign in: {CanSignIn}", canSignIn);

            if (!canSignIn)
            {
                return BadRequest("User cannot sign in - check email confirmation, lockout, etc.");
            }

            // Try to sign in
            var signInResult = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, lockoutOnFailure: false);

            _logger.LogInformation("Sign in result: Succeeded={Succeeded}, IsLockedOut={IsLockedOut}, RequiresTwoFactor={RequiresTwoFactor}, IsNotAllowed={IsNotAllowed}",
                signInResult.Succeeded, signInResult.IsLockedOut, signInResult.RequiresTwoFactor, signInResult.IsNotAllowed);

            return Ok(new
            {
                PasswordValid = passwordCheck,
                CanSignIn = canSignIn,
                SignInResult = new
                {
                    signInResult.Succeeded,
                    signInResult.IsLockedOut,
                    signInResult.RequiresTwoFactor,
                    signInResult.IsNotAllowed
                }
            });
        }

        [HttpPost("test-login-email")]
        public async Task<IActionResult> TestLoginEmail([FromBody] LoginTestModel model)
        {
            _logger.LogInformation("Testing login for email: {Email}", model.Email);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            // Check password
            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.Password);
            _logger.LogInformation("Password check result: {PasswordValid}", passwordCheck);

            if (!passwordCheck)
            {
                return BadRequest("Invalid password");
            }

            // Check if user can sign in
            var canSignIn = await _signInManager.CanSignInAsync(user);
            _logger.LogInformation("Can sign in: {CanSignIn}", canSignIn);

            if (!canSignIn)
            {
                return BadRequest("User cannot sign in - check email confirmation, lockout, etc.");
            }

            // Try to sign in
            var signInResult = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: false);

            _logger.LogInformation("Sign in result: Succeeded={Succeeded}, IsLockedOut={IsLockedOut}, RequiresTwoFactor={RequiresTwoFactor}, IsNotAllowed={IsNotAllowed}",
                signInResult.Succeeded, signInResult.IsLockedOut, signInResult.RequiresTwoFactor, signInResult.IsNotAllowed);

            return Ok(new
            {
                PasswordValid = passwordCheck,
                CanSignIn = canSignIn,
                SignInResult = new
                {
                    signInResult.Succeeded,
                    signInResult.IsLockedOut,
                    signInResult.RequiresTwoFactor,
                    signInResult.IsNotAllowed
                }
            });
        }

        [HttpPost("debug-signin")]
        public async Task<IActionResult> DebugSignIn([FromBody] LoginTestModel model)
        {
            string email = model.Email;
            string password = model.Password;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound("User not found");
            }
        
            // Check password validity
            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            
            // Check user status
            var canSignIn = await _signInManager.CanSignInAsync(user);
            
            // Check if user requires 2FA
            var requiresTwoFactor = await _userManager.GetTwoFactorEnabledAsync(user);
            
            // Check user roles
            var roles = await _userManager.GetRolesAsync(user);
            
            // Check authentication schemes
            var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();

            var signInResult = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: false);
        
              // Try signing in with USERNAME
            var signInResultUsername = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, lockoutOnFailure: false);

            return Ok(new
            {
                UserFound = true,
                UserId = user.Id,
                Email = user.Email,
                NormalizedEmail = user.NormalizedEmail,
                UserName = user.UserName,
                NormalizedUserName = user.NormalizedUserName,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                PasswordValid = passwordValid,
                CanSignIn = canSignIn,
                RequiresTwoFactor = requiresTwoFactor,
                Roles = roles,
                ExternalSchemes = schemes.Select(s => s.Name).ToList(),
                SecurityStamp = user.SecurityStamp,
                signInResult = new
                {
                    signInResult.Succeeded,
                    signInResult.IsLockedOut,
                    signInResult.RequiresTwoFactor,
                    signInResult.IsNotAllowed
                },
                signInResultUsername = new
                {
                    signInResultUsername.Succeeded,
                    signInResultUsername.IsLockedOut,
                    signInResultUsername.RequiresTwoFactor,
                    signInResultUsername.IsNotAllowed
                }
            });
        }
    }
    public class LoginTestModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}