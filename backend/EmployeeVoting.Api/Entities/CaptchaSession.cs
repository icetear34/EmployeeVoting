using System;

namespace EmployeeVoting.Api.Entities
{
    /// <summary>
    /// 驗證碼 Session 實體
    /// </summary>
    public class CaptchaSession
    {
        /// <summary>
        /// 主鍵
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 對外識別碼
        /// </summary>
        public string CaptchaId { get; set; } = string.Empty;

        /// <summary>
        /// 驗證值
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 用途（employee-login / admin-login）
        /// </summary>
        public string Purpose { get; set; } = string.Empty;

        /// <summary>
        /// 到期時間（建議 3-5 分鐘）
        /// </summary>
        public DateTime ExpireAt { get; set; }

        /// <summary>
        /// 是否已使用
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
