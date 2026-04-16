namespace EmployeeVoting.Api.Dtos.Admin
{
    /// <summary>
    /// 管理者登入請求
    /// </summary>
    public class AdminLoginRequest
    {
        /// <summary>
        /// 帳號
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 密碼
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 驗證碼 Id
        /// </summary>
        public string CaptchaId { get; set; } = string.Empty;

        /// <summary>
        /// 驗證碼
        /// </summary>
        public string CaptchaCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// 管理者登入回應
    /// </summary>
    public class AdminLoginResponse
    {
        /// <summary>
        /// Session Token
        /// </summary>
        public string SessionToken { get; set; } = string.Empty;

        /// <summary>
        /// 帳號
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 顯示名稱
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 管理者資訊回應（/me 端點）
    /// </summary>
    public class AdminMeResponse
    {
        /// <summary>
        /// 帳號
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 顯示名稱
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
    }
}
