using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Insightly.Models;
using Insightly.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Insightly.Areas.Identity.Pages.Account
{
    public class VerifyCodeModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<VerifyCodeModel> _logger;

        public VerifyCodeModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IVerificationCodeService verificationCodeService,
            IEmailSender emailSender,
            ILogger<VerifyCodeModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _verificationCodeService = verificationCodeService;
            _emailSender = emailSender;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string UserEmail { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Please enter the verification code")]
            [StringLength(5, MinimumLength = 5, ErrorMessage = "Verification code must be 5 digits")]
            [RegularExpression(@"^\d{5}$", ErrorMessage = "Verification code must be 5 digits")]
            [Display(Name = "Verification Code")]
            public string VerificationCode { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get user information from TempData
            if (TempData["UserId"] == null || TempData["UserEmail"] == null)
            {
                return RedirectToPage("./Register");
            }

            UserEmail = TempData["UserEmail"].ToString();

            // Keep the data for potential resubmission
            TempData.Keep("UserId");
            TempData.Keep("UserEmail");
            TempData.Keep("ReturnUrl");

            return Page();
        }

        // Default POST handler - matches asp-page-handler="VerifyCode" in the form
        public async Task<IActionResult> OnPostVerifyCodeAsync()
        {
            if (!ModelState.IsValid)
            {
                UserEmail = TempData["UserEmail"]?.ToString();
                TempData.Keep("UserId");
                TempData.Keep("UserEmail");
                TempData.Keep("ReturnUrl");
                return Page();
            }

            var userId = TempData["UserId"]?.ToString();
            var userEmail = TempData["UserEmail"]?.ToString();
            var returnUrl = TempData["ReturnUrl"]?.ToString() ?? "~/";

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("./Register");
            }

            // Validate the verification code
            var isValid = await _verificationCodeService.ValidateCodeAsync(userId, Input.VerificationCode);

            if (isValid)
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    // Mark email as confirmed
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("User verified their email successfully.");

                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Clear TempData
                    TempData.Clear();

                    // Show success message
                    TempData["SuccessMessage"] = "Your email has been verified successfully!";

                    return LocalRedirect(returnUrl);
                }
            }

            // Invalid code
            ModelState.AddModelError(string.Empty, "Invalid verification code. Please try again.");
            UserEmail = userEmail;

            // Keep the data for retry
            TempData.Keep("UserId");
            TempData.Keep("UserEmail");
            TempData.Keep("ReturnUrl");

            return Page();
        }

        // Resend code handler - matches asp-page-handler="ResendCode" in the form
        public async Task<IActionResult> OnPostResendCodeAsync()
        {
            var userId = TempData["UserId"]?.ToString();
            var userEmail = TempData["UserEmail"]?.ToString();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("./Register");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                // Invalidate old code
                await _verificationCodeService.InvalidateCodeAsync(userId);

                // Generate new code
                var newCode = await _verificationCodeService.GenerateCodeAsync(userId);

                // Send email with new code
                var emailSubject = "New Verification Code";
                var emailBody = $@"
<!DOCTYPE html>
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
                    <span style=""color: white; font-size: 36px; letter-spacing: 12px; font-weight: 600;"">{newCode}</span>
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

                await _emailSender.SendEmailAsync(userEmail, emailSubject, emailBody);

                TempData["InfoMessage"] = "A new verification code has been sent to your email.";

                _logger.LogInformation($"New verification code sent to {userEmail}");
            }

            UserEmail = userEmail;

            // Keep the data for next attempt
            TempData.Keep("UserId");
            TempData.Keep("UserEmail");
            TempData.Keep("ReturnUrl");

            return Page();
        }
    }
}