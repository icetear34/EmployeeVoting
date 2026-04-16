using System;

namespace EmployeeVoting.Api.Entities
{
    /// <summary>
    /// 可投票名單實體
    /// </summary>
    public class EligibleVoter
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
        /// 工號
        /// </summary>
        public string EmployeeNo { get; set; } = string.Empty;

        /// <summary>
        /// 生日（標準化後 yyyyMMdd）
        /// </summary>
        public string BirthDate { get; set; } = string.Empty;

        /// <summary>
        /// 匯入時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
