using EmployeeVoting.Api.Dtos.Admin;

namespace EmployeeVoting.Api.Application.Interfaces
{
    /// <summary>
    /// 管理者帳號服務介面
    /// </summary>
    public interface IAdminUserService
    {
        /// <summary>分頁查詢管理者帳號列表</summary>
        Task<PagedResult<AdminUserResponse>> GetPagedAsync(AdminUserQueryRequest query);

        /// <summary>依 Id 查詢</summary>
        Task<AdminUserResponse?> GetByIdAsync(Guid id);

        /// <summary>新增管理者帳號</summary>
        Task<AdminUserResponse> CreateAsync(CreateAdminUserRequest request);

        /// <summary>更新管理者帳號（含可選密碼）</summary>
        Task<AdminUserResponse> UpdateAsync(Guid id, UpdateAdminUserRequest request);

        /// <summary>重設密碼</summary>
        Task ResetPasswordAsync(Guid id, ResetAdminPasswordRequest request);

        /// <summary>刪除管理者帳號</summary>
        Task DeleteAsync(Guid id);
    }
}
