namespace EmployeeVoting.Api.Dtos.Admin
{
    /// <summary>
    /// 活動分區基本資訊
    /// </summary>
    public class ActivityGroupResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        /// <summary>分區內的管理帳號數</summary>
        public int AdminCount { get; set; }
        /// <summary>分區內的活動數</summary>
        public int ActivityCount { get; set; }
    }

    /// <summary>
    /// 活動分區詳情（含成員列表）
    /// </summary>
    public class ActivityGroupDetailResponse : ActivityGroupResponse
    {
        public List<AdminUserResponse> Members { get; set; } = new();
    }

    /// <summary>
    /// 建立分區請求
    /// </summary>
    public class CreateActivityGroupRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Guid> AdminUserIds { get; set; } = new();
    }

    /// <summary>
    /// 更新分區請求
    /// </summary>
    public class UpdateActivityGroupRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// 設定分區成員請求
    /// </summary>
    public class SetGroupMembersRequest
    {
        public List<Guid> AdminUserIds { get; set; } = new();
    }
}
