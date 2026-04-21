namespace EmployeeVoting.Api.Infrastructure.Configuration
{
    /// <summary>
    /// 應用程式設定
    /// </summary>
    public class AppSettings
    {
        public const string SectionName = "AppSettings";

        /// <summary>
        /// Session 過期時間（分鐘）
        /// </summary>
        public int SessionExpireMinutes { get; set; } = 60*24*7;

        /// <summary>
        /// 驗證碼過期時間（分鐘）
        /// </summary>
        public int CaptchaExpireMinutes { get; set; } = 5;

        /// <summary>
        /// 圖片最大大小（bytes）
        /// </summary>
        public int MaxImageSizeBytes { get; set; } = 2097152; // 2MB

        /// <summary>
        /// 允許的圖片副檔名
        /// </summary>
        public string[] AllowedImageExtensions { get; set; } = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        /// <summary>
        /// 上傳路徑
        /// </summary>
        public string UploadPath { get; set; } = "wwwroot/uploads/candidates";
    }
}
