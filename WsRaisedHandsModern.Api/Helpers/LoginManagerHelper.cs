using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using WsRaisedHandsModern.Api.Data.AppData.Entities;

namespace WsRaisedHandsModern.Api.Helpers
{
    public class LoginManagerHelper
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<LoginManagerHelper> _logger;

        public LoginManagerHelper()
        {
            // Constructor logic if needed
        }
        /*public async bool LoginEmail(LoginTestModel model)
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
        }*/
    }
}