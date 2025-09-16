// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Insightly.Models;
using Insightly.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace Insightly.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IVerificationCodeService _verificationCodeService;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IVerificationCodeService verificationCodeService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _verificationCodeService = verificationCodeService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Full Name")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            public string Name { get; set; }

            [Required]
            [Display(Name = "Gender")]
            public string Gender { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.Name = Input.Name;
                user.Gender = Input.Gender;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _userManager.AddToRoleAsync(user, "User");

                    // Generate 5-digit verification code
                    var userId = await _userManager.GetUserIdAsync(user);
                    var verificationCode = await _verificationCodeService.GenerateCodeAsync(userId);

                    // Send email with verification code
                    var emailSubject = "Verify your email - Your verification code";
                    var emailBody = $@"
                                 <html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f5f5f5; padding: 0; margin: 0;"">
    <div style=""max-width: 600px; margin: 40px auto; background-color: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);"">
        <!-- Header -->
        <div style=""background-color: #ff5747; padding: 30px; text-align: center;"">
            <h1 style=""color: white; margin: 0; font-size: 32px; font-weight: 600;"">Insightly</h1>
        </div>
        
        <!-- Content -->
        <div style=""padding: 40px;"">
            <!-- Title -->
            <h2 style=""color: #2c3e50; text-align: center; font-weight: 600; margin: 0 0 30px 0; font-size: 28px;"">New Verification Code</h2>
            
            <!-- Greeting -->
            <p style=""color: #5a6c7d; font-size: 16px; line-height: 1.6; margin-bottom: 20px;"">Hello {user.Name},</p>
            
            <!-- Message -->
            <p style=""color: #5a6c7d; font-size: 16px; line-height: 1.6; margin-bottom: 30px; text-align: center;"">Here is your new verification code:</p>
            
            <!-- Code Display -->
            <div style=""text-align: center; margin: 35px 0;"">
                <div style=""background-color: #ff5747; display: inline-block; padding: 20px 40px; border-radius: 8px;"">
                    <span style=""color: white; font-size: 36px; letter-spacing: 12px; font-weight: 600;"">{verificationCode}</span>
                </div>
            </div>
            
            <!-- Warning Box -->
            <div style=""background-color: #fef5f5; border: 1px solid #fde0dd; padding: 15px; border-radius: 6px; margin: 30px 0;"">
                <p style=""color: #d8504c; font-size: 14px; margin: 0; text-align: center;"">
                    If you didn't request this verification code, please ignore this email.
                </p>
            </div>
            
            <!-- Expiration Notice -->
            <p style=""color: #7a8a9a; font-size: 14px; text-align: center; margin-top: 25px;"">
                This code will expire in <strong>15 minutes</strong>.
            </p>
        </div>
    </div>
</body>
</html>";

                    await _emailSender.SendEmailAsync(Input.Email, emailSubject, emailBody);

                    // Store user ID in TempData to use in verification page
                    TempData["UserId"] = userId;
                    TempData["UserEmail"] = Input.Email;
                    TempData["ReturnUrl"] = returnUrl;

                    // Redirect to verification page
                    return RedirectToPage("VerifyCode");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}