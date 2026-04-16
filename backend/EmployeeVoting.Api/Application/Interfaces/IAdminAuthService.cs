using EmployeeVoting.Api.Dtos.Admin;
using EmployeeVoting.Api.Domain.Entities;

namespace EmployeeVoting.Api.Application.Interfaces
{
    /// <summary>
    /// 管理者認證服務介面
    /// </summary>
    public interface IAdminAuthService
    {
        /// <summary>
        /// 管理者登入
        /// </summary>
        Task<(AdminLoginResponse? response, string? errorCode, string? errorMessage)> LoginAsync(AdminLoginRequest request);

        /// <summary>
        /// 取得目前登入資訊
        /// </summary>
        Task<(AdminMeResponse? response, string? errorCode)> GetCurrentUserAsync(string sessionToken);

        /// <summary>
        /// 登出
        /// </summary>
        Task LogoutAsync(string sessionToken);

        /// <summary>
        /// 驗證 Session
        /// </summary>
        Task<(bool isValid, SessionToken? session)> ValidateSessionAsync(string sessionToken);
    }
}
