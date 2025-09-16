using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Insightly.Services
{
    public class VerificationCodeService : IVerificationCodeService
    {
        private readonly IMemoryCache _cache;
        private readonly Random _random = new Random();

        public VerificationCodeService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<string> GenerateCodeAsync(string userId, string purpose = "EmailConfirmation")
        {
            // Generate a 5-digit code
            var code = _random.Next(10000, 99999).ToString();

            // Store the code in cache with 15 minutes expiration
            var cacheKey = $"VerificationCode_{purpose}_{userId}";
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

            _cache.Set(cacheKey, code, cacheOptions);

            return await Task.FromResult(code);
        }

        public async Task<bool> ValidateCodeAsync(string userId, string code, string purpose = "EmailConfirmation")
        {
            var cacheKey = $"VerificationCode_{purpose}_{userId}";

            if (_cache.TryGetValue(cacheKey, out string cachedCode))
            {
                if (cachedCode == code)
                {
                    // Remove the code after successful validation
                    _cache.Remove(cacheKey);
                    return await Task.FromResult(true);
                }
            }

            return await Task.FromResult(false);
        }

        public async Task InvalidateCodeAsync(string userId, string purpose = "EmailConfirmation")
        {
            var cacheKey = $"VerificationCode_{purpose}_{userId}";
            _cache.Remove(cacheKey);
            await Task.CompletedTask;
        }
    }
}