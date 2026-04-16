namespace EmployeeVoting.Api.Dtos.Admin
{
    /// <summary>
    /// 可投票名單項目
    /// </summary>
    public class EligibleVoterListItem
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 工號
        /// </summary>
        public string EmployeeNo { get; set; } = string.Empty;

        /// <summary>
        /// 生日（yyyyMMdd）
        /// </summary>
        public string BirthDate { get; set; } = string.Empty;

        /// <summary>
        /// 匯入時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 匯入可投票名單請求
    /// </summary>
    public class ImportEligibleVotersRequest
    {
        /// <summary>
        /// 名單文字（每行格式：工號 空格 生日）
        /// </summary>
        public string Data { get; set; } = string.Empty;
    }

    /// <summary>
    /// 匯入結果回應
    /// </summary>
    public class ImportEligibleVotersResponse
    {
        /// <summary>
        /// 成功筆數
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失敗筆數
        /// </summary>
        public int FailCount { get; set; }

        /// <summary>
        /// 錯誤明細
        /// </summary>
        public List<ImportError> Errors { get; set; } = new();
    }

    /// <summary>
    /// 匯入錯誤明細
    /// </summary>
    public class ImportError
    {
        /// <summary>
        /// 行號
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// 原始資料
        /// </summary>
        public string RawData { get; set; } = string.Empty;

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
