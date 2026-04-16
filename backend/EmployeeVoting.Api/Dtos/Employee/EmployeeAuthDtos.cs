namespace EmployeeVoting.Api.Dtos.Employee
{
    /// <summary>
    /// 員工登入請求
    /// </summary>
    public class EmployeeLoginRequest
    {
        /// <summary>
        /// 工號
        /// </summary>
        public string EmployeeNo { get; set; } = string.Empty;

        /// <summary>
        /// 生日（yyyyMMdd 格式）
        /// </summary>
        public string BirthDate { get; set; } = string.Empty;

        /// <summary>
        /// 驗證碼 Id
        /// </summary>
        public string CaptchaId { get; set; } = string.Empty;

        /// <summary>
        /// 驗證碼
        /// </summary>
        public string CaptchaCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// 員工登入回應
    /// </summary>
    public class EmployeeLoginResponse
    {
        /// <summary>
        /// Session Token
        /// </summary>
        public string SessionToken { get; set; } = string.Empty;

        /// <summary>
        /// 工號
        /// </summary>
        public string EmployeeNo { get; set; } = string.Empty;

        /// <summary>
        /// 有效活動數量
        /// </summary>
        public int ActivityCount { get; set; }

        /// <summary>
        /// 是否所有活動都已投票
        /// </summary>
        public bool AllVoted { get; set; }

        /// <summary>
        /// 活動列表
        /// </summary>
        public List<ActivityWithCandidatesDto> Activities { get; set; } = new();
    }

    /// <summary>
    /// 活動與候選人資訊
    /// </summary>
    public class ActivityWithCandidatesDto
    {
        /// <summary>
        /// 活動 Id
        /// </summary>
        public Guid ActivityId { get; set; }

        /// <summary>
        /// 活動代號
        /// </summary>
        public string ActivityCode { get; set; } = string.Empty;

        /// <summary>
        /// 活動名稱
        /// </summary>
        public string ActivityName { get; set; } = string.Empty;

        /// <summary>
        /// 活動說明
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
        /// 是否已投票
        /// </summary>
        public bool HasVoted { get; set; }

        /// <summary>
        /// 候選人列表
        /// </summary>
        public List<CandidateDto> Candidates { get; set; } = new();

        /// <summary>
        /// 得票佔比（已投票時才有）
        /// </summary>
        public List<ResultBarDto>? ResultBars { get; set; }
    }

    /// <summary>
    /// 候選人資訊
    /// </summary>
    public class CandidateDto
    {
        /// <summary>
        /// 候選人 Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 介紹
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 圖片網址
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// 得票佔比
    /// </summary>
    public class ResultBarDto
    {
        /// <summary>
        /// 候選人姓名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 得票百分比
        /// </summary>
        public decimal Percent { get; set; }
    }

    /// <summary>
    /// 員工資訊回應（/me 端點）
    /// </summary>
    public class EmployeeMeResponse
    {
        /// <summary>
        /// 工號
        /// </summary>
        public string EmployeeNo { get; set; } = string.Empty;

        /// <summary>
        /// 是否所有活動都已投票
        /// </summary>
        public bool AllVoted { get; set; }

        /// <summary>
        /// 活動列表
        /// </summary>
        public List<ActivityWithCandidatesDto> Activities { get; set; } = new();
    }
}
