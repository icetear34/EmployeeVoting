using System;

namespace EmployeeVoting.Api.Entities
{
    /// <summary>
    /// 管理者帳號實體
    /// </summary>
    public class AdminUser
    {
        /// <summary>
        /// 主鍵
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 登入帳號，唯一
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 密碼（明文儲存 - 依規格決策）
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 顯示名稱
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 啟用狀態
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
}
