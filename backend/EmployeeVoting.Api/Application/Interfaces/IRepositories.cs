using EmployeeVoting.Api.Domain.Entities;
using EmployeeVoting.Api.Dtos.Admin;

namespace EmployeeVoting.Api.Application.Interfaces
{
    /// <summary>
    /// 投票活動 Repository 介面
    /// </summary>
    public interface IVoteActivityRepository
    {
        Task<VoteActivity?> GetByIdAsync(Guid id);
        Task<VoteActivity?> GetByActivityCodeAsync(string activityCode);
        Task<IEnumerable<VoteActivity>> GetAllAsync(bool includeDeleted = false);
        Task<IEnumerable<VoteActivity>> GetActiveActivitiesAsync();

        /// <summary>
        /// 分頁查詢（含搜尋、狀態過濾、排序）
        /// </summary>
        Task<(IEnumerable<VoteActivity> Items, int TotalCount)> GetPagedAsync(ActivityQueryRequest query);

        Task<Guid> CreateAsync(VoteActivity activity);
        Task UpdateAsync(VoteActivity activity);
        Task SoftDeleteAsync(Guid id);
        Task<bool> ActivityCodeExistsAsync(string activityCode, Guid? excludeId = null);
        Task<string> GenerateActivityCodeAsync();
    }

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

    /// <summary>
    /// 候選人 Repository 介面
    /// </summary>
    public interface ICandidateRepository
    {
        Task<IEnumerable<Candidate>> GetByActivityIdAsync(Guid activityId);
        Task<Candidate?> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(Candidate candidate);
        Task BatchCreateAsync(IEnumerable<Candidate> candidates);
        Task DeleteByActivityIdAsync(Guid activityId);
        Task ReplaceAllAsync(Guid activityId, IEnumerable<Candidate> candidates);
    }

    /// <summary>
    /// 可投票名單 Repository 介面
    /// </summary>
    public interface IEligibleVoterRepository
    {
        Task<IEnumerable<EligibleVoter>> GetByActivityIdAsync(Guid activityId);
        Task<int> GetCountByActivityIdAsync(Guid activityId);
        Task<EligibleVoter?> FindAsync(Guid activityId, string employeeNo);
        Task BatchCreateAsync(IEnumerable<EligibleVoter> voters);
        Task DeleteByActivityIdAsync(Guid activityId);
        Task ReplaceAllAsync(Guid activityId, IEnumerable<EligibleVoter> voters);
    }
}
