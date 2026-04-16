namespace EmployeeVoting.Api.Dtos.Common
{
    /// <summary>
    /// 驗證碼回應
    /// </summary>
    public class CaptchaResponse
    {
        /// <summary>
        /// 驗證碼 Id
        /// </summary>
        public string CaptchaId { get; set; } = string.Empty;

        /// <summary>
        /// 驗證碼圖片 Base64
        /// </summary>
        public string ImageBase64 { get; set; } = string.Empty;
    }

    /// <summary>
    /// 通用錯誤回應
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 詳細資訊
        /// </summary>
        public object? Details { get; set; }
    }

    /// <summary>
    /// 通用成功回應
    /// </summary>
    public class SuccessResponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 分頁請求
    /// </summary>
    public class PagedRequest
    {
        /// <summary>
        /// 頁碼（從 1 開始）
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        public string? Keyword { get; set; }
    }

    /// <summary>
    /// 分頁回應
    /// </summary>
    public class PagedResponse<T>
    {
        /// <summary>
        /// 資料列表
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// 總筆數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 目前頁碼
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
