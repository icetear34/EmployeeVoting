namespace EmployeeVoting.Api.Domain.Entities
{
    /// <summary>
    /// 投票活動實體
    /// </summary>
    public class VoteActivity
    {
        public Guid Id { get; set; }
        public string ActivityCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// 候選人實體
    /// </summary>
    public class Candidate
    {
        public Guid Id { get; set; }
        public Guid VoteActivityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 可投票名單實體
    /// </summary>
    public class EligibleVoter
    {
        public Guid Id { get; set; }
        public Guid VoteActivityId { get; set; }
        public string EmployeeNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string BirthDate { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 投票紀錄實體
    /// </summary>
    public class VoteRecord
    {
        public Guid Id { get; set; }
        public Guid VoteActivityId { get; set; }
        public Guid CandidateId { get; set; }
        public string EmployeeNo { get; set; } = string.Empty;
        public DateTime VotedAt { get; set; }
        public string ClientIp { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }

    /// <summary>
    /// 管理者帳號實體
    /// </summary>
    public class AdminUser
    {
        public Guid Id { get; set; }
        public string Account { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Session 令牌實體
    /// </summary>
    public class SessionToken
    {
        public Guid Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? EmployeeNo { get; set; }
        public Guid? AdminUserId { get; set; }
        public Guid? CurrentVoteActivityId { get; set; }
        public DateTime ExpireAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRevoked { get; set; }
    }

    /// <summary>
    /// 驗證碼 Session 實體
    /// </summary>
    public class CaptchaSession
    {
        public Guid Id { get; set; }
        public string CaptchaId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public DateTime ExpireAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
