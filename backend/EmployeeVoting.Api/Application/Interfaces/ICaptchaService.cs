using EmployeeVoting.Api.Dtos.Common;

namespace EmployeeVoting.Api.Application.Interfaces
{
    /// <summary>
    /// 驗證碼服務介面
    /// </summary>
    public interface ICaptchaService
    {
        /// <summary>
        /// 產生驗證碼
        /// </summary>
        Task<CaptchaResponse> GenerateAsync(string purpose);

        /// <summary>
        /// 驗證驗證碼
        /// </summary>
        Task<(bool isValid, string? errorCode)> ValidateAsync(string captchaId, string code, string purpose);
    }
}
