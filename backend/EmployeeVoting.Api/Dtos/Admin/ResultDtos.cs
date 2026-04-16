namespace EmployeeVoting.Api.Dtos.Admin
{
    /// <summary>
    /// 開票統計摘要
    /// </summary>
    public class ResultSummaryResponse
    {
        /// <summary>
        /// 總可投票人數
        /// </summary>
        public int TotalVoters { get; set; }

        /// <summary>
        /// 已投票人數
        /// </summary>
        public int VotedCount { get; set; }

        /// <summary>
        /// 未投票人數
        /// </summary>
        public int NotVotedCount { get; set; }

        /// <summary>
        /// 投票率（百分比）
        /// </summary>
        public decimal VotingRate { get; set; }

        /// <summary>
        /// 各候選人得票資料
        /// </summary>
        public List<CandidateResultItem> Results { get; set; } = new();
    }

    /// <summary>
    /// 候選人得票結果
    /// </summary>
    public class CandidateResultItem
    {
        /// <summary>
        /// 排名
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 候選人 Id
        /// </summary>
        public Guid CandidateId { get; set; }

        /// <summary>
        /// 候選人姓名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 得票數
        /// </summary>
        public int VoteCount { get; set; }

        /// <summary>
        /// 得票比例（百分比）
        /// </summary>
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// 圓餅圖資料回應
    /// </summary>
    public class ResultChartResponse
    {
        /// <summary>
        /// 圖表資料
        /// </summary>
        public List<ChartDataItem> Data { get; set; } = new();
    }

    /// <summary>
    /// 圖表資料項目
    /// </summary>
    public class ChartDataItem
    {
        /// <summary>
        /// 標籤（候選人姓名）
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// 數值（得票數）
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// 百分比
        /// </summary>
        public decimal Percentage { get; set; }
    }
}
