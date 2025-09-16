// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Azure.Core;
using Insightly.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
using System.ComponentModel.DataAnnotations;
using System.Composition;
using System.Data;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Channels;
using System.Threading.Tasks;

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
                // In your C# code (e.g., an EmailService class)
                // Make sure you have the callbackUrl variable defined
                string emailBody = $"""
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Reset Your Password</title>
                </head>
                <body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, Segoe UI, Roboto, Helvetica Neue, Arial, sans-serif; background-color: #f8f9fb;'>
                    <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%' style='background-color: #f8f9fb;'>
                        <tr>
                            <td align='center' style='padding: 40px 20px;'>
                                <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; width: 100%; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05); border: 1px solid #f1f3f5;'>
                                    <tr>
                                        <td style='background-color: #ff4b37; padding: 30px; text-align: center; border-radius: 12px 12px 0 0;'>
                                            <h1 style='margin: 0; color: #ffffff; font-size: 28px; font-weight: 700;'>
                                                Insightly
                                            </h1>
                                        </td>
                                    </tr>
                                    
                                    <tr>
                                        <td style='padding: 40px 30px;'>
                                            <h2 style='margin: 0 0 20px 0; color: #212529; font-size: 24px; font-weight: 600; text-align: center;'>
                                                Reset Your Password
                                            </h2>
                                            <p style='margin: 0 0 30px 0; color: #495057; font-size: 16px; line-height: 24px; text-align: center;'>
                                                We received a request to reset the password for your account. If you made this request, click the button below to continue.
                                            </p>
                                            
                                            <div style='text-align: center; margin: 35px 0;'>
                                                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'
                                                   style='display: inline-block; padding: 14px 32px; background-color: #ff4b37;
                                                          color: #ffffff; text-decoration: none; font-size: 16px; font-weight: 600; border-radius: 8px;
                                                          box-shadow: 0 4px 14px 0 rgba(255, 75, 55, 0.4);'>
                                                    Reset My Password
                                                </a>
                                            </div>
                                            
                                            <div style='background-color: #fff7f5; border-left: 4px solid #ffd9d3; padding: 15px; border-radius: 6px; margin: 30px 0;'>
                                                <p style='margin: 0; color: #a83224; font-size: 14px; line-height: 20px;'>
                                                    If you did not request a password reset, please ignore this email. Your password will not be changed.
                                                </p>
                                            </div>
                                            
                                            <p style='margin: 30px 0 0 0; color: #6c757d; font-size: 14px; line-height: 20px; text-align: center;'>
                                                This link will expire in <strong>24 hours</strong>.
                                            </p>
                                        </td>
                                    </tr>
                                    
                                    <tr>
                                        <td style='background-color: #f8f9fa; padding: 30px; text-align: center; border-radius: 0 0 12px 12px;'>
                                            <p style='margin: 0; color: #6c757d; font-size: 12px;'>
                                                © {DateTime.Now.Year} Articles Portal. All rights reserved.
                                            </p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>
                """;

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Your Password - Instghtly",
                    emailBody);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}