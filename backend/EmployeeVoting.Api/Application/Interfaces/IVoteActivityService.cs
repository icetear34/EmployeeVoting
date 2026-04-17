using EmployeeVoting.Api.Dtos.Admin;

namespace EmployeeVoting.Api.Application.Interfaces
{
    /// <summary>
    /// 投票活動服務介面
    /// </summary>
    public interface IVoteActivityService
    {
        /// <summary>
        /// 取得所有活動列表
        /// </summary>
        Task<IEnumerable<VoteActivityListItem>> GetAllActivitiesAsync();

        /// <summary>
        /// 取得活動詳情
        /// </summary>
        Task<VoteActivityDetailResponse?> GetActivityByIdAsync(Guid id);

        /// <summary>
        /// 建立新活動
        /// </summary>
        Task<VoteActivityDetailResponse> CreateActivityAsync(CreateVoteActivityRequest request, string createdBy);

        /// <summary>
        /// 更新活動
        /// </summary>
        Task<VoteActivityDetailResponse> UpdateActivityAsync(Guid id, UpdateVoteActivityRequest request);

        /// <summary>
        /// 刪除活動（軟刪除）
        /// </summary>
        Task DeleteActivityAsync(Guid id);
    }
}
