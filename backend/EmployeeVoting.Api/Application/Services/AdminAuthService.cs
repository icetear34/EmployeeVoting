using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Common;
using EmployeeVoting.Api.Domain.Entities;
using EmployeeVoting.Api.Dtos.Admin;
using EmployeeVoting.Api.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace EmployeeVoting.Api.Application.Services
{
    /// <summary>
    /// 管理者認證服務實作
    /// </summary>
    public class AdminAuthService : IAdminAuthService
    {
        private readonly IAdminUserRepository _adminUserRepository;
        private readonly ISessionTokenRepository _sessionTokenRepository;
        private readonly ICaptchaService _captchaService;
        private readonly AppSettings _settings;

        public AdminAuthService(
            IAdminUserRepository adminUserRepository,
            ISessionTokenRepository sessionTokenRepository,
            ICaptchaService captchaService,
            IOptions<AppSettings> settings)
        {
            _adminUserRepository = adminUserRepository;
            _sessionTokenRepository = sessionTokenRepository;
            _captchaService = captchaService;
            _settings = settings.Value;
        }

        public async Task<(AdminLoginResponse? response, string? errorCode, string? errorMessage)> LoginAsync(AdminLoginRequest request)
        {
            // 1. 驗證驗證碼
            var (captchaValid, captchaError) = await _captchaService.ValidateAsync(
                request.CaptchaId,
                request.CaptchaCode,
                CaptchaPurpose.AdminLogin);

            if (!captchaValid)
            {
                var message = captchaError == ErrorCodes.CaptchaExpired ? "驗證碼已過期" : "驗證碼錯誤";
                return (null, captchaError, message);
            }

            // 2. 驗證帳號密碼
            var admin = await _adminUserRepository.GetByAccountAsync(request.Account);

            if (admin == null)
            {
                return (null, ErrorCodes.InvalidCredentials, "帳號或密碼錯誤");
            }

            // 密碼明文比對（依規格要求）
            if (admin.Password != request.Password)
            {
                return (null, ErrorCodes.InvalidCredentials, "帳號或密碼錯誤");
            }

            // 3. 檢查帳號是否啟用
            if (!admin.IsEnabled)
            {
                return (null, ErrorCodes.AccountDisabled, "此帳號已被停用");
            }

            // 4. 建立 Session
            var session = new SessionToken
            {
                Role = UserRoles.Admin,
                AdminUserId = admin.Id,
                ExpireAt = DateTime.Now.AddMinutes(_settings.SessionExpireMinutes)
            };

            var token = await _sessionTokenRepository.CreateAsync(session);

            // 5. 回傳結果
            return (new AdminLoginResponse
            {
                SessionToken = token,
                Account = admin.Account,
                DisplayName = admin.DisplayName
            }, null, null);
        }

        public async Task<(AdminMeResponse? response, string? errorCode)> GetCurrentUserAsync(string sessionToken)
        {
            var (isValid, session) = await ValidateSessionAsync(sessionToken);

            if (!isValid || session == null)
            {
                return (null, ErrorCodes.SessionExpired);
            }

            if (session.AdminUserId == null)
            {
                return (null, ErrorCodes.Unauthorized);
            }

            var admin = await _adminUserRepository.GetByIdAsync(session.AdminUserId.Value);

            if (admin == null)
            {
                return (null, ErrorCodes.Unauthorized);
            }

            if (!admin.IsEnabled)
            {
                return (null, ErrorCodes.AccountDisabled);
            }

            return (new AdminMeResponse
            {
                Account = admin.Account,
                DisplayName = admin.DisplayName,
                Role = admin.Role
            }, null);
        }

        public async Task LogoutAsync(string sessionToken)
        {
            if (!string.IsNullOrWhiteSpace(sessionToken))
            {
                await _sessionTokenRepository.RevokeAsync(sessionToken);
            }
        }

        public async Task<(bool isValid, SessionToken? session)> ValidateSessionAsync(string sessionToken)
        {
            if (string.IsNullOrWhiteSpace(sessionToken))
            {
                return (false, null);
            }

            var session = await _sessionTokenRepository.GetByTokenAsync(sessionToken);

            if (session == null)
            {
                return (false, null);
            }

            if (session.IsRevoked)
            {
                return (false, null);
            }

            if (session.ExpireAt < DateTime.Now)
            {
                return (false, null);
            }

            if (session.Role != UserRoles.Admin)
            {
                return (false, null);
            }

            return (true, session);
        }
    }
}
