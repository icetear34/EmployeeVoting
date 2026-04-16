using System;

namespace EmployeeVoting.Api.Entities
{
    /// <summary>
    /// 投票紀錄實體
    /// </summary>
    public class VoteRecord
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
        /// 候選人 Id（FK → Candidate）
        /// </summary>
        public Guid CandidateId { get; set; }

        /// <summary>
        /// 工號
        /// </summary>
        public string EmployeeNo { get; set; } = string.Empty;

        /// <summary>
        /// 投票時間
        /// </summary>
        public DateTime VotedAt { get; set; }

        /// <summary>
        /// 投票來源 IP
        /// </summary>
        public string ClientIp { get; set; } = string.Empty;

        /// <summary>
        /// 使用者代理
        /// </summary>
        public string UserAgent { get; set; } = string.Empty;
    }
}
