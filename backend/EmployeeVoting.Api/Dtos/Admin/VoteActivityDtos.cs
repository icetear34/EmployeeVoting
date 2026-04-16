namespace EmployeeVoting.Api.Dtos.Admin
{
    /// <summary>
    /// 活動列表項目
    /// </summary>
    public class VoteActivityListItem
    {
        /// <summary>
        /// 活動 Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 活動代號
        /// </summary>
        public string ActivityCode { get; set; } = string.Empty;

        /// <summary>
        /// 活動名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

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
        /// 狀態（未開始 / 進行中 / 已結束）
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 新增活動請求
    /// </summary>
    public class CreateVoteActivityRequest
    {
        /// <summary>
        /// 活動名稱（必填）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 活動說明（選填）
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 開始時間（必填）
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 結束時間（必填）
        /// </summary>
        public DateTime EndTime { get; set; }
    }

    /// <summary>
    /// 更新活動請求
    /// </summary>
    public class UpdateVoteActivityRequest
    {
        /// <summary>
        /// 活動名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

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
    }

    /// <summary>
    /// 活動詳情回應
    /// </summary>
    public class VoteActivityDetailResponse
    {
        /// <summary>
        /// 活動 Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 活動代號
        /// </summary>
        public string ActivityCode { get; set; } = string.Empty;

        /// <summary>
        /// 活動名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

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
        /// 狀態
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 建立人
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;
    }
}
