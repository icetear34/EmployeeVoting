using EmployeeVoting.Api.Dtos.Admin;

namespace EmployeeVoting.Api.Application.Interfaces
{
    /// <summary>
    /// 活動分區服務介面
    /// </summary>
    public interface IActivityGroupService
    {
        Task<IEnumerable<ActivityGroupResponse>> GetAllAsync();
        Task<ActivityGroupDetailResponse?> GetByIdAsync(Guid id);
        Task<ActivityGroupResponse> CreateAsync(CreateActivityGroupRequest request, string createdBy);
        Task<ActivityGroupResponse> UpdateAsync(Guid id, UpdateActivityGroupRequest request);
        Task DeleteAsync(Guid id);
        Task SetMembersAsync(Guid groupId, IEnumerable<Guid> adminUserIds);
    }
}
