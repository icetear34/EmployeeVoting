using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Common;
using EmployeeVoting.Api.Domain.Entities;
using EmployeeVoting.Api.Dtos.Employee;
using EmployeeVoting.Api.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace EmployeeVoting.Api.Application.Services
{
    /// <summary>
    /// 員工認證服務實作
    /// </summary>
    public class EmployeeAuthService : IEmployeeAuthService
    {
        private readonly IEligibleVoterRepository _eligibleVoterRepository;
        private readonly ISessionTokenRepository _sessionTokenRepository;
        private readonly ICaptchaService _captchaService;
        private readonly IEmployeeVoteService _employeeVoteService;
        private readonly AppSettings _settings;

        public EmployeeAuthService(
            IEligibleVoterRepository eligibleVoterRepository,
            ISessionTokenRepository sessionTokenRepository,
            ICaptchaService captchaService,
            IEmployeeVoteService employeeVoteService,
            IOptions<AppSettings> settings)
        {
            _eligibleVoterRepository = eligibleVoterRepository;
            _sessionTokenRepository = sessionTokenRepository;
            _captchaService = captchaService;
            _employeeVoteService = employeeVoteService;
            _settings = settings.Value;
        }

        public async Task<(EmployeeLoginResponse? response, string? errorCode, string? errorMessage)> LoginAsync(EmployeeLoginRequest request)
        {
            // 1. 驗證驗證碼
            var (captchaValid, captchaError) = await _captchaService.ValidateAsync(
                request.CaptchaId,
                request.CaptchaCode,
                CaptchaPurpose.EmployeeLogin);

            if (!captchaValid)
            {
                var message = captchaError == ErrorCodes.CaptchaExpired ? "驗證碼已過期" : "驗證碼錯誤";
                return (null, captchaError, message);
            }

            // 2. 查詢工號是否在任一活動的投票名單中
            var voters = (await _eligibleVoterRepository.GetByEmployeeNoAsync(request.EmployeeNo)).ToList();

            if (voters.Count == 0)
            {
                return (null, ErrorCodes.InvalidCredentials, "工號或生日錯誤");
            }

            // 3. 比對生日（移除橫線與空白後純數字比對，如 1990-01-01 → 19900101）
            static string NormalizeBirth(string s) => s.Replace("-", "").Replace("/", "").Replace(" ", "");
            var normalizedRequest = NormalizeBirth(request.BirthDate);
            var matchedVoter = voters.FirstOrDefault(v => NormalizeBirth(v.BirthDate) == normalizedRequest);
            if (matchedVoter == null)
            {
                return (null, ErrorCodes.InvalidCredentials, "工號或生日錯誤");
            }

            // 4. 建立 Session
            var session = new SessionToken
            {
                Role = UserRoles.Employee,
                EmployeeNo = request.EmployeeNo,
                ExpireAt = DateTime.UtcNow.AddMinutes(_settings.SessionExpireMinutes)
            };

            var token = await _sessionTokenRepository.CreateAsync(session);

            // 5. 取得活動資訊（進行中 或 已投票且結果可查閱）
            var activities = await _employeeVoteService.GetActivitiesForEmployeeAsync(request.EmployeeNo);

            // 若無任何可顯示的活動，視為登入失敗
            if (activities.Count == 0)
            {
                // 已建立的 session 撤銷，避免殘留
                await _sessionTokenRepository.RevokeAsync(token);
                return (null, ErrorCodes.ActivityNotActive, "目前沒有進行中的活動，也沒有可查閱的結果");
            }

            var allVoted = activities.All(a => a.HasVoted);

            return (new EmployeeLoginResponse
            {
                SessionToken = token,
                EmployeeNo = request.EmployeeNo,
                ActivityCount = activities.Count,
                AllVoted = allVoted,
                Activities = activities
            }, null, null);
        }

        public async Task<(EmployeeMeResponse? response, string? errorCode)> GetCurrentUserAsync(string sessionToken)
        {
            var (isValid, session) = await ValidateSessionAsync(sessionToken);

            if (!isValid || session == null)
            {
                return (null, ErrorCodes.SessionExpired);
            }

            if (string.IsNullOrWhiteSpace(session.EmployeeNo))
            {
                return (null, ErrorCodes.Unauthorized);
            }

            var activities = await _employeeVoteService.GetActivitiesForEmployeeAsync(session.EmployeeNo);
            var allVoted = activities.Count > 0 && activities.All(a => a.HasVoted);

            return (new EmployeeMeResponse
            {
                EmployeeNo = session.EmployeeNo,
                AllVoted = allVoted,
                Activities = activities
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

            if (session == null || session.IsRevoked || session.ExpireAt < DateTime.UtcNow)
            {
                return (false, null);
            }

            if (session.Role != UserRoles.Employee)
            {
                return (false, null);
            }

            return (true, session);
        }
    }
}
