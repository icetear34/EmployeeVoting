using System;

namespace EmployeeVoting.Api.Entities
{
    /// <summary>
    /// Session 令牌實體
    /// </summary>
    public class SessionToken
    {
        /// <summary>
        /// 主鍵
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 唯一 token
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// 角色（employee / admin）
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// 員工工號（可空）
        /// </summary>
        public string? EmployeeNo { get; set; }

        /// <summary>
        /// 管理者 Id（可空）
        /// </summary>
        public Guid? AdminUserId { get; set; }

        /// <summary>
        /// 目前選定活動 Id（可空）
        /// </summary>
        public Guid? CurrentVoteActivityId { get; set; }

        /// <summary>
        /// 到期時間
        /// </summary>
        public DateTime ExpireAt { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 是否失效
        /// </summary>
        public bool IsRevoked { get; set; }
    }
}
