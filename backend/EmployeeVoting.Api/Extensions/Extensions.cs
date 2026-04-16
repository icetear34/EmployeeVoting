using EmployeeVoting.Api.Common;

namespace EmployeeVoting.Api.Extensions
{
    /// <summary>
    /// 活動狀態擴充方法
    /// </summary>
    public static class ActivityStatusExtensions
    {
        /// <summary>
        /// 根據時間計算活動狀態
        /// </summary>
        public static ActivityStatus GetStatus(DateTime startTime, DateTime endTime)
        {
            var now = DateTime.UtcNow;
            if (now < startTime)
                return ActivityStatus.NotStarted;
            if (now > endTime)
                return ActivityStatus.Ended;
            return ActivityStatus.InProgress;
        }

        /// <summary>
        /// 取得狀態顯示文字
        /// </summary>
        public static string GetStatusText(this ActivityStatus status)
        {
            return status switch
            {
                ActivityStatus.NotStarted => "未開始",
                ActivityStatus.InProgress => "進行中",
                ActivityStatus.Ended => "已結束",
                _ => "未知"
            };
        }
    }

    /// <summary>
    /// 日期格式擴充方法
    /// </summary>
    public static class DateExtensions
    {
        /// <summary>
        /// 標準化生日格式為 yyyyMMdd
        /// </summary>
        public static string? NormalizeBirthDate(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // 移除所有非數字字元
            var digitsOnly = new string(input.Where(char.IsDigit).ToArray());

            // 如果已經是 8 位數字，直接返回
            if (digitsOnly.Length == 8)
                return digitsOnly;

            // 嘗試解析各種日期格式
            var formats = new[]
            {
                "yyyy/MM/dd",
                "yyyy-MM-dd",
                "yyyy.MM.dd",
                "yyyyMMdd"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(input.Trim(), format, 
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, 
                    out var date))
                {
                    return date.ToString("yyyyMMdd");
                }
            }

            return null;
        }
    }

    /// <summary>
    /// 密碼驗證擴充方法
    /// </summary>
    public static class PasswordExtensions
    {
        /// <summary>
        /// 驗證密碼是否符合複雜度規範
        /// - 長度至少 8 碼
        /// - 至少 1 碼大寫英文字母
        /// - 至少 1 碼小寫英文字母
        /// - 至少 1 碼數字
        /// - 至少 1 碼特殊字元
        /// </summary>
        public static bool IsValidPassword(string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(password))
            {
                errorMessage = "密碼不可為空";
                return false;
            }

            if (password.Length < 8)
            {
                errorMessage = "密碼長度至少需要 8 碼";
                return false;
            }

            if (!password.Any(char.IsUpper))
            {
                errorMessage = "密碼需包含至少 1 碼大寫英文字母";
                return false;
            }

            if (!password.Any(char.IsLower))
            {
                errorMessage = "密碼需包含至少 1 碼小寫英文字母";
                return false;
            }

            if (!password.Any(char.IsDigit))
            {
                errorMessage = "密碼需包含至少 1 碼數字";
                return false;
            }

            var specialChars = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~";
            if (!password.Any(c => specialChars.Contains(c)))
            {
                errorMessage = "密碼需包含至少 1 碼特殊字元";
                return false;
            }

            return true;
        }
    }
}
