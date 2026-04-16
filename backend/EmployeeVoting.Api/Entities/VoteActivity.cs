using System;

namespace EmployeeVoting.Api.Entities
{
    /// <summary>
    /// 投票活動實體
    /// </summary>
    public class VoteActivity
    {
        /// <summary>
        /// 主鍵
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 活動代號，唯一（格式：VOTE-YYYYMMDD-XXXX）
        /// </summary>
        public string ActivityCode { get; set; } = string.Empty;

        /// <summary>
        /// 活動名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 活動說明（選填，可為 null）
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 建立人
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// 軟刪除標記
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
