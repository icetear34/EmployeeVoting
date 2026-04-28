namespace EmployeeVoting.Api.Dtos.Admin
{
    /// <summary>
    /// 管理者帳號回應（前端相容欄位名）
    /// </summary>
    public class AdminUserResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = "admin";
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        /// <summary>所屬分區列表（僅 super_admin 操作時回傳）</summary>
        public List<ActivityGroupResponse> Groups { get; set; } = new();
    }

    /// <summary>
    /// 管理者分頁查詢請求
    /// </summary>
    public class AdminUserQueryRequest
    {
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// 新增管理者請求
    /// </summary>
    public class CreateAdminUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = "admin";
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// 更新管理者請求
    /// </summary>
    public class UpdateAdminUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = "admin";
        public bool IsEnabled { get; set; } = true;
        public string? Password { get; set; }
    }

    /// <summary>
    /// 重設密碼請求
    /// </summary>
    public class ResetAdminPasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
