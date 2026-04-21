namespace EmployeeVoting.Api.Dtos.Admin
{
    /// <summary>
    /// 活動列表項目
    /// </summary>
    public class VoteActivityListItem
    {
        public Guid Id { get; set; }
        public string ActivityCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 新增活動請求（一次送出基本資料 + 候選人 + 投票名單）
    /// </summary>
    public class CreateVoteActivityRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 候選人列表（可為空，之後再補）
        /// </summary>
        public List<CreateActivityCandidateItem> Candidates { get; set; } = new();

        /// <summary>
        /// 投票名單（可為空，之後再補）
        /// </summary>
        public List<CreateActivityVoterItem> EligibleVoters { get; set; } = new();
    }

    /// <summary>
    /// 新增活動時的候選人項目
    /// </summary>
    public class CreateActivityCandidateItem
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// 新增活動時的投票人項目
    /// </summary>
    public class CreateActivityVoterItem
    {
        public string EmployeeNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string BirthDate { get; set; } = string.Empty;
    }

    /// <summary>
    /// 更新活動基本資料請求（僅基本資料，不含候選人/投票名單）
    /// </summary>
    public class UpdateVoteActivityRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    /// <summary>
    /// 編輯模式 - 整批更新候選人請求
    /// </summary>
    public class UpdateCandidatesRequest
    {
        public List<CandidateEditItem> Candidates { get; set; } = new();
    }

    /// <summary>
    /// 候選人編輯項目（有 Id 表示更新，無 Id 表示新增）
    /// </summary>
    public class CandidateEditItem
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// 編輯模式 - 整批更新投票名單請求
    /// </summary>
    public class UpdateEligibleVotersRequest
    {
        public List<VoterEditItem> Voters { get; set; } = new();
    }

    /// <summary>
    /// 投票人編輯項目
    /// </summary>
    public class VoterEditItem
    {
        public string EmployeeNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string BirthDate { get; set; } = string.Empty;
    }

    /// <summary>
    /// 活動詳情回應（含候選人 + 投票名單）
    /// </summary>
    public class VoteActivityDetailResponse
    {
        public Guid Id { get; set; }
        public string ActivityCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// 候選人列表
        /// </summary>
        public List<CandidateListItem> Candidates { get; set; } = new();

        /// <summary>
        /// 投票名單
        /// </summary>
        public List<EligibleVoterListItem> EligibleVoters { get; set; } = new();
    }
}
