using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Insightly.Services
{
    public class VerificationCodeService : IVerificationCodeService
    {
        private readonly IMemoryCache _cache;

        public VerificationCodeService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<string> GenerateCodeAsync(string userId, string purpose = "EmailConfirmation")
        {
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            var cacheKey = $"VerificationCode_{purpose}_{userId}";
            _cache.Set(cacheKey, code, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15)));

            return await Task.FromResult(code);
        }

        public async Task<bool> ValidateCodeAsync(string userId, string code, string purpose = "EmailConfirmation")
        {
            var cacheKey = $"VerificationCode_{purpose}_{userId}";

            if (_cache.TryGetValue(cacheKey, out string cachedCode) && cachedCode == code)
            {
                _cache.Remove(cacheKey);
                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }

        public async Task InvalidateCodeAsync(string userId, string purpose = "EmailConfirmation")
        {
            _cache.Remove($"VerificationCode_{purpose}_{userId}");
            await Task.CompletedTask;
        }
    }
}