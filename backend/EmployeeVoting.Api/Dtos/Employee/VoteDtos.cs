namespace EmployeeVoting.Api.Dtos.Employee
{
    /// <summary>
    /// 單筆投票項目
    /// </summary>
    public class VoteItem
    {
        /// <summary>
        /// 活動 Id
        /// </summary>
        public Guid ActivityId { get; set; }

        /// <summary>
        /// 候選人 Id
        /// </summary>
        public Guid CandidateId { get; set; }
    }

    /// <summary>
    /// 批次投票請求
    /// </summary>
    public class SubmitBatchVoteRequest
    {
        /// <summary>
        /// 投票列表
        /// </summary>
        public List<VoteItem> Votes { get; set; } = new();
    }

    /// <summary>
    /// 投票回應
    /// </summary>
    public class VoteResponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 活動得票佔比回應
    /// </summary>
    public class ActivityResultBarsResponse
    {
        /// <summary>
        /// 各候選人得票佔比
        /// </summary>
        public List<ResultBarDto> Candidates { get; set; } = new();
    }
}
