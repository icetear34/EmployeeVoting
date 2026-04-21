using EmployeeVoting.Api.Dtos.Admin;

namespace EmployeeVoting.Api.Application.Interfaces
{
    /// <summary>
    /// 投票活動服務介面
    /// </summary>
    public interface IVoteActivityService
    {
        Task<IEnumerable<VoteActivityListItem>> GetAllActivitiesAsync();
        Task<VoteActivityDetailResponse?> GetActivityByIdAsync(Guid id);
        Task<VoteActivityDetailResponse> CreateActivityAsync(CreateVoteActivityRequest request, string createdBy);
        Task<VoteActivityDetailResponse> UpdateActivityAsync(Guid id, UpdateVoteActivityRequest request);
        Task DeleteActivityAsync(Guid id);

        /// <summary>
        /// 編輯模式 - 整批更新候選人
        /// </summary>
        Task UpdateCandidatesAsync(Guid activityId, UpdateCandidatesRequest request);

        /// <summary>
        /// 編輯模式 - 整批更新投票名單
        /// </summary>
        Task UpdateEligibleVotersAsync(Guid activityId, UpdateEligibleVotersRequest request);
    }
}
