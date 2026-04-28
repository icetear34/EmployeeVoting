using EmployeeVoting.Api.Dtos.Admin;

namespace EmployeeVoting.Api.Application.Interfaces
{
    /// <summary>
    /// 投票活動服務介面
    /// </summary>
    public interface IVoteActivityService
    {
        /// <summary>分頁查詢活動列表（含搜尋/過濾/排序）</summary>
        /// <param name="adminUserId">操作者 Id（admin 角色時限制分區）</param>
        /// <param name="adminRole">操作者角色（super_admin 可看全部）</param>
        Task<PagedResult<VoteActivityListItem>> GetActivitiesAsync(ActivityQueryRequest query, Guid? adminUserId = null, string adminRole = "admin");

        Task<IEnumerable<VoteActivityListItem>> GetAllActivitiesAsync();
        Task<VoteActivityDetailResponse?> GetActivityByIdAsync(Guid id);

        /// <param name="adminUserId">建立者 Id（admin 自動歸入其分區）</param>
        /// <param name="adminRole">建立者角色</param>
        Task<VoteActivityDetailResponse> CreateActivityAsync(CreateVoteActivityRequest request, string createdBy, Guid? adminUserId = null, string adminRole = "admin");

        Task<VoteActivityDetailResponse> UpdateActivityAsync(Guid id, UpdateVoteActivityRequest request);
        Task DeleteActivityAsync(Guid id);

        /// <summary>編輯模式 - 整批更新候選人</summary>
        Task UpdateCandidatesAsync(Guid activityId, UpdateCandidatesRequest request);

        /// <summary>編輯模式 - 整批更新投票名單</summary>
        Task UpdateEligibleVotersAsync(Guid activityId, UpdateEligibleVotersRequest request);
    }
}
