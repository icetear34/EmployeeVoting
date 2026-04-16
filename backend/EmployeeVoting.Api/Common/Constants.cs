namespace EmployeeVoting.Api.Common
{
    /// <summary>
    /// 活動狀態
    /// </summary>
    public enum ActivityStatus
    {
        /// <summary>
        /// 未開始
        /// </summary>
        NotStarted,

        /// <summary>
        /// 進行中
        /// </summary>
        InProgress,

        /// <summary>
        /// 已結束
        /// </summary>
        Ended
    }

    /// <summary>
    /// 使用者角色
    /// </summary>
    public static class UserRoles
    {
        public const string Employee = "employee";
        public const string Admin = "admin";
    }

    /// <summary>
    /// 驗證碼用途
    /// </summary>
    public static class CaptchaPurpose
    {
        public const string EmployeeLogin = "employee-login";
        public const string AdminLogin = "admin-login";
    }

    /// <summary>
    /// Cookie 名稱
    /// </summary>
    public static class CookieNames
    {
        public const string EmployeeSession = "employee_session";
        public const string AdminSession = "admin_session";
    }

    /// <summary>
    /// 錯誤代碼
    /// </summary>
    public static class ErrorCodes
    {
        // 認證相關
        public const string InvalidCaptcha = "INVALID_CAPTCHA";
        public const string CaptchaExpired = "CAPTCHA_EXPIRED";
        public const string InvalidCredentials = "INVALID_CREDENTIALS";
        public const string SessionExpired = "SESSION_EXPIRED";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string AccountDisabled = "ACCOUNT_DISABLED";

        // 投票相關
        public const string NoEligibleActivity = "NO_ELIGIBLE_ACTIVITY";
        public const string AlreadyVoted = "ALREADY_VOTED";
        public const string ActivityNotActive = "ACTIVITY_NOT_ACTIVE";
        public const string InvalidCandidate = "INVALID_CANDIDATE";
        public const string NotInVoterList = "NOT_IN_VOTER_LIST";
        public const string BatchVoteFailed = "BATCH_VOTE_FAILED";

        // 資料相關
        public const string NotFound = "NOT_FOUND";
        public const string DuplicateData = "DUPLICATE_DATA";
        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string InvalidFormat = "INVALID_FORMAT";

        // 系統相關
        public const string InternalError = "INTERNAL_ERROR";
    }
}
