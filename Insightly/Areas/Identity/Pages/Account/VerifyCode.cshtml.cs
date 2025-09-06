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
                    <html>
                    <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                        <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                            <h2 style='color: #333; text-align: center;'>New Verification Code</h2>
                            <p style='color: #666; font-size: 16px;'>Hello {user.Name},</p>
                            <p style='color: #666; font-size: 16px;'>Here is your new verification code:</p>
                            <div style='background-color: #f8f9fb; padding: 20px; border-radius: 8px; text-align: center; margin: 20px 0;'>
                                <h1 style='color: #007bff; letter-spacing: 8px; font-size: 36px; margin: 0;'>{newCode}</h1>
                            </div>
                            <p style='color: #999; font-size: 14px; text-align: center;'>This code will expire in 15 minutes</p>
                            <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                            <p style='color: #999; font-size: 12px; text-align: center;'>If you didn't request this verification code, please ignore this email.</p>
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