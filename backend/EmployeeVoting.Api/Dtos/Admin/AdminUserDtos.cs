namespace EmployeeVoting.Api.Dtos.Admin
{
    /// <summary>
    /// 管理者列表項目
    /// </summary>
    public class AdminUserListItem
    {
        /// <summary>
        /// 管理者 Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 帳號
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 顯示名稱
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 新增管理者請求
    /// </summary>
    public class CreateAdminUserRequest
    {
        /// <summary>
        /// 帳號（必填）
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 密碼（必填，需符合複雜度規範）
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 顯示名稱（必填）
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 更新管理者請求
    /// </summary>
    public class UpdateAdminUserRequest
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

    /// <summary>
    /// 修改密碼請求
    /// </summary>
    public class ChangePasswordRequest
    {
        /// <summary>
        /// 新密碼（需符合複雜度規範）
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// 修改狀態請求
    /// </summary>
    public class ChangeStatusRequest
    {
        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
