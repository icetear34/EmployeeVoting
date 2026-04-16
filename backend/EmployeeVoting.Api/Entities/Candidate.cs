using System;

namespace EmployeeVoting.Api.Entities
{
    /// <summary>
    /// 候選人實體
    /// </summary>
    public class Candidate
    {
        /// <summary>
        /// 主鍵
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 所屬投票活動 Id（FK → VoteActivity）
        /// </summary>
        public Guid VoteActivityId { get; set; }

        /// <summary>
        /// 候選人姓名（必填）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 介紹（可多行文字）
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 圖片路徑（必填）
        /// </summary>
        public string ImagePath { get; set; } = string.Empty;

        /// <summary>
        /// 排序號
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 啟用狀態
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
