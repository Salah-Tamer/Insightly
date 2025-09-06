// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Insightly.Models;

namespace Insightly.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // Generate password reset token
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                // Create a beautiful HTML email template
                var emailBody = $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Reset Your Password</title>
                    <!--[if mso]>
                    <noscript>
                        <xml>
                            <o:OfficeDocumentSettings>
                                <o:PixelsPerInch>96</o:PixelsPerInch>
                            </o:OfficeDocumentSettings>
                        </xml>
                    </noscript>
                    <![endif]-->
                </head>
                <body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #f8f9fb;'>
                    <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%' style='background-color: #f8f9fb;'>
                        <tr>
                            <td align='center' style='padding: 40px 20px;'>
                                <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; width: 100%; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.07);'>
                                    <!-- Header -->
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 30px; text-align: center; border-radius: 12px 12px 0 0;'>
                                            <h1 style='margin: 0; color: #ffffff; font-size: 32px; font-weight: 700; letter-spacing: -0.5px;'>
                                                Insightly
                                            </h1>
                                            <p style='margin: 10px 0 0 0; color: rgba(255, 255, 255, 0.9); font-size: 16px;'>
                                                Password Reset Request
                                            </p>
                                        </td>
                                    </tr>
                                    
                                    <!-- Content -->
                                    <tr>
                                        <td style='padding: 40px 30px;'>
                                            <!-- Icon -->
                                            <div style='text-align: center; margin-bottom: 30px;'>
                                                <div style='display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border-radius: 50%; padding: 20px;'>
                                                    <svg width='40' height='40' viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                                                        <path d='M12 2L2 7L12 12L22 7L12 2Z' stroke='white' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
                                                        <path d='M2 17L12 22L22 17' stroke='white' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
                                                        <path d='M2 12L12 17L22 12' stroke='white' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
                                                    </svg>
                                                </div>
                                            </div>
                                            
                                            <h2 style='margin: 0 0 20px 0; color: #1a202c; font-size: 24px; font-weight: 600; text-align: center;'>
                                                Reset Your Password
                                            </h2>
                                            
                                            <p style='margin: 0 0 20px 0; color: #4a5568; font-size: 16px; line-height: 24px;'>
                                                Hello,
                                            </p>
                                            
                                            <p style='margin: 0 0 30px 0; color: #4a5568; font-size: 16px; line-height: 24px;'>
                                                We received a request to reset the password for your account associated with <strong>{HtmlEncoder.Default.Encode(Input.Email)}</strong>. 
                                                If you made this request, click the button below to reset your password:
                                            </p>
                                            
                                            <!-- CTA Button -->
                                            <div style='text-align: center; margin: 35px 0;'>
                                                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                                                   style='display: inline-block; padding: 14px 32px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                                                          color: #ffffff; text-decoration: none; font-size: 16px; font-weight: 600; border-radius: 8px; 
                                                          box-shadow: 0 4px 14px 0 rgba(102, 126, 234, 0.4);'>
                                                    Reset My Password
                                                </a>
                                            </div>
                                            
                                            <p style='margin: 30px 0 0 0; color: #718096; font-size: 14px; line-height: 20px;'>
                                                This password reset link will expire in <strong>24 hours</strong> for security reasons.
                                            </p>
                                        </td>
                                    </tr>
                                    
                                    <!-- Footer -->
                                    <tr>
                                        <td style='background-color: #f8f9fb; padding: 30px; text-align: center; border-radius: 0 0 12px 12px;'>
                                            <p style='margin: 0 0 10px 0; color: #a0aec0; font-size: 13px;'>
                                                Need help? Contact our support team
                                            </p>
                                            <p style='margin: 0 0 10px 0;'>
                                                <a href='mailto:support@articlesportal.com' style='color: #667eea; text-decoration: none; font-size: 13px;'>
                                                    support@articlesportal.com
                                                </a>
                                            </p>
                                            <div style='margin: 20px 0 10px 0; padding-top: 20px; border-top: 1px solid #e2e8f0;'>
                                                <p style='margin: 0; color: #a0aec0; font-size: 12px;'>
                                                    © 2025 Articles Portal. All rights reserved.
                                                </p>
                                                <p style='margin: 5px 0 0 0; color: #a0aec0; font-size: 12px;'>
                                                    You're receiving this email because a password reset was requested for your account.
                                                </p>
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Your Password - Articles Portal",
                    emailBody);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}