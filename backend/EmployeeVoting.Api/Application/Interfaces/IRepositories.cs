using EmployeeVoting.Api.Domain.Entities;

namespace EmployeeVoting.Api.Application.Interfaces
{
    /// <summary>
    /// 管理者帳號 Repository 介面
    /// </summary>
    public interface IAdminUserRepository
    {
        Task<AdminUser?> GetByAccountAsync(string account);
        Task<AdminUser?> GetByIdAsync(Guid id);
        Task<IEnumerable<AdminUser>> GetAllAsync();
        Task<Guid> CreateAsync(AdminUser adminUser);
        Task UpdateAsync(AdminUser adminUser);
        Task<bool> AccountExistsAsync(string account, Guid? excludeId = null);
    }

    /// <summary>
    /// Session Token Repository 介面
    /// </summary>
    public interface ISessionTokenRepository
    {
        Task<SessionToken?> GetByTokenAsync(string token);
        Task<string> CreateAsync(SessionToken session);
        Task RevokeAsync(string token);
        Task RevokeAllByAdminUserIdAsync(Guid adminUserId);
        Task RevokeAllByEmployeeNoAsync(string employeeNo);
        Task CleanupExpiredAsync();
    }

    /// <summary>
    /// 驗證碼 Session Repository 介面
    /// </summary>
    public interface ICaptchaSessionRepository
    {
        Task<CaptchaSession?> GetByCaptchaIdAsync(string captchaId);
        Task<string> CreateAsync(CaptchaSession captchaSession);
        Task MarkAsUsedAsync(string captchaId);
        Task CleanupExpiredAsync();
    }
}
