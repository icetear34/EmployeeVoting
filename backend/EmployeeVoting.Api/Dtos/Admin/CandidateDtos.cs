namespace EmployeeVoting.Api.Dtos.Admin
{
    /// <summary>
    /// 候選人列表項目
    /// </summary>
    public class CandidateListItem
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

        /// <summary>
        /// 排序號
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 得票數
        /// </summary>
        public int VoteCount { get; set; }
    }

    /// <summary>
    /// 新增候選人請求（使用 multipart/form-data）
    /// </summary>
    public class CreateCandidateRequest
    {
        /// <summary>
        /// 姓名（必填）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 介紹
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 排序號
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 圖片檔案
        /// </summary>
        public IFormFile? Image { get; set; }
    }

    /// <summary>
    /// 更新候選人請求
    /// </summary>
    public class UpdateCandidateRequest
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 介紹
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 排序號
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 新圖片檔案（選填）
        /// </summary>
        public IFormFile? Image { get; set; }
    }
}
