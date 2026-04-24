using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Common;
using EmployeeVoting.Api.Domain.Entities;
using EmployeeVoting.Api.Dtos.Common;
using EmployeeVoting.Api.Infrastructure.Configuration;
using EmployeeVoting.Api.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace EmployeeVoting.Api.Application.Services
{
    /// <summary>
    /// 驗證碼服務實作
    /// </summary>
    public class CaptchaService : ICaptchaService
    {
        private readonly ICaptchaSessionRepository _captchaRepository;
        private readonly ICaptchaImageGenerator _imageGenerator;
        private readonly AppSettings _settings;
        private static readonly Random _random = new();
        private const string CaptchaChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public CaptchaService(
            ICaptchaSessionRepository captchaRepository,
            ICaptchaImageGenerator imageGenerator,
            IOptions<AppSettings> settings)
        {
            _captchaRepository = captchaRepository;
            _imageGenerator = imageGenerator;
            _settings = settings.Value;
        }

        public async Task<CaptchaResponse> GenerateAsync(string purpose)
        {
            // 產生隨機驗證碼（4碼）
            var code = GenerateRandomCode(4);

            // 產生圖片
            var imageBase64 = _imageGenerator.Generate(code);

            // 儲存到資料庫
            var captchaSession = new CaptchaSession
            {
                Code = code.ToUpper(),
                Purpose = purpose,
                ExpireAt = DateTime.Now.AddMinutes(_settings.CaptchaExpireMinutes)
            };

            var captchaId = await _captchaRepository.CreateAsync(captchaSession);

            return new CaptchaResponse
            {
                CaptchaId = captchaId,
                ImageBase64 = imageBase64
            };
        }

        public async Task<(bool isValid, string? errorCode)> ValidateAsync(string captchaId, string code, string purpose)
        {
            if (string.IsNullOrWhiteSpace(captchaId) || string.IsNullOrWhiteSpace(code))
            {
                return (false, ErrorCodes.InvalidCaptcha);
            }

            var captchaSession = await _captchaRepository.GetByCaptchaIdAsync(captchaId);

            if (captchaSession == null)
            {
                return (false, ErrorCodes.InvalidCaptcha);
            }

            if (captchaSession.IsUsed)
            {
                return (false, ErrorCodes.InvalidCaptcha);
            }

            if (captchaSession.ExpireAt < DateTime.Now)
            {
                return (false, ErrorCodes.CaptchaExpired);
            }

            if (captchaSession.Purpose != purpose)
            {
                return (false, ErrorCodes.InvalidCaptcha);
            }

            if (!string.Equals(captchaSession.Code, code.ToUpper(), StringComparison.OrdinalIgnoreCase))
            {
                return (false, ErrorCodes.InvalidCaptcha);
            }

            // 標記為已使用
            await _captchaRepository.MarkAsUsedAsync(captchaId);

            return (true, null);
        }

        private static string GenerateRandomCode(int length)
        {
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = CaptchaChars[_random.Next(CaptchaChars.Length)];
            }
            return new string(chars);
        }
    }
}
