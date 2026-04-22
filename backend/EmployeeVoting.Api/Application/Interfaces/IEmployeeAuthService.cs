using EmployeeVoting.Api.Domain.Entities;
using EmployeeVoting.Api.Dtos.Employee;

namespace EmployeeVoting.Api.Application.Interfaces
{
    /// <summary>
    /// 員工認證服務介面
    /// </summary>
    public interface IEmployeeAuthService
    {
        Task<(EmployeeLoginResponse? response, string? errorCode, string? errorMessage)> LoginAsync(EmployeeLoginRequest request);
        Task<(EmployeeMeResponse? response, string? errorCode)> GetCurrentUserAsync(string sessionToken);
        Task LogoutAsync(string sessionToken);
        Task<(bool isValid, SessionToken? session)> ValidateSessionAsync(string sessionToken);
    }
}
