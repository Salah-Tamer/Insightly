using System;
using System.Threading.Tasks;

namespace Insightly.Services
{
    public interface IVerificationCodeService
    {
        Task<string> GenerateCodeAsync(string userId, string purpose = "EmailConfirmation");
        Task<bool> ValidateCodeAsync(string userId, string code, string purpose = "EmailConfirmation");
        Task InvalidateCodeAsync(string userId, string purpose = "EmailConfirmation");
    }
}