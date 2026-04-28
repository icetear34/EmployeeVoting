using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Common;
using EmployeeVoting.Api.Domain.Exceptions;
using EmployeeVoting.Api.Dtos.Admin;
using EmployeeVoting.Api.Dtos.Common;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeVoting.Api.Controllers
{
    /// <summary>
    /// 投票活動管理 API
    /// </summary>
    [Route("api/admin/activities")]
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class VoteActivityController : AdminBaseController
    {
        private readonly IVoteActivityService _voteActivityService;
        private readonly IAdminUserRepository _adminUserRepository;

        public VoteActivityController(
            IVoteActivityService voteActivityService,
            IAdminUserRepository adminUserRepository)
        {
            _voteActivityService = voteActivityService;
            _adminUserRepository = adminUserRepository;
        }

        /// <summary>
        /// 取得活動列表（分頁 + 搜尋 + 狀態過濾 + 排序）
        /// </summary>
        /// <param name="keyword">文字搜尋（活動名稱）</param>
        /// <param name="status">狀態：pending / active / ended</param>
        /// <param name="sortBy">排序欄位：createdAt / startTime / endTime / name</param>
        /// <param name="sortDir">排序方向：asc / desc</param>
        /// <param name="page">頁碼（從 1 開始）</param>
        /// <param name="pageSize">每頁筆數（預設 10，最大 100）</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<VoteActivityListItem>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActivities(
            [FromQuery] string? keyword,
            [FromQuery] string? status,
            [FromQuery] string? sortBy   = "createdAt",
            [FromQuery] string? sortDir  = "desc",
            [FromQuery] int     page     = 1,
            [FromQuery] int     pageSize = 10)
        {
            var query = new ActivityQueryRequest
            {
                Keyword  = keyword,
                Status   = status,
                SortBy   = sortBy,
                SortDir  = sortDir,
                Page     = page,
                PageSize = pageSize
            };

            var result = await _voteActivityService.GetActivitiesAsync(query, CurrentSession?.AdminUserId, CurrentSession?.Role ?? "admin");
            return Ok(result);
        }

        /// <summary>
        /// 取得活動詳情
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(VoteActivityDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActivity(Guid id)
        {
            var activity = await _voteActivityService.GetActivityByIdAsync(id);
            
            if (activity == null)
            {
                return NotFound(new ErrorResponse
                {
                    Code = ErrorCodes.NotFound,
                    Message = "活動不存在"
                });
            }
            
            return Ok(activity);
        }

        /// <summary>
        /// 建立新活動
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(VoteActivityDetailResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateActivity([FromBody] CreateVoteActivityRequest request)
        {
            try
            {
                // 取得當前管理者名稱
                var createdBy = "admin";
                if (CurrentAdminUserId.HasValue)
                {
                    var adminUser = await _adminUserRepository.GetByIdAsync(CurrentAdminUserId.Value);
                    if (adminUser != null)
                    {
                        createdBy = adminUser.DisplayName ?? adminUser.Account;
                    }
                }

                var activity = await _voteActivityService.CreateActivityAsync(request, createdBy, CurrentSession?.AdminUserId, CurrentSession?.Role ?? "admin");
                
                return CreatedAtAction(
                    nameof(GetActivity), 
                    new { id = activity.Id }, 
                    activity);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Code = ErrorCodes.ValidationFailed,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 更新活動
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(VoteActivityDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateActivity(Guid id, [FromBody] UpdateVoteActivityRequest request)
        {
            try
            {
                var activity = await _voteActivityService.UpdateActivityAsync(id, request);
                return Ok(activity);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ErrorResponse
                {
                    Code = ErrorCodes.NotFound,
                    Message = ex.Message
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Code = ErrorCodes.ValidationFailed,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 刪除活動
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteActivity(Guid id)
        {
            try
            {
                await _voteActivityService.DeleteActivityAsync(id);
                
                return Ok(new SuccessResponse
                {
                    Success = true,
                    Message = "活動已刪除"
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ErrorResponse
                {
                    Code = ErrorCodes.NotFound,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 編輯模式 - 整批更新候選人
        /// </summary>
        [HttpPut("{id}/candidates")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCandidates(Guid id, [FromBody] UpdateCandidatesRequest request)
        {
            try
            {
                await _voteActivityService.UpdateCandidatesAsync(id, request);
                return Ok(new SuccessResponse { Success = true, Message = "候選人已更新" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ErrorResponse { Code = ErrorCodes.NotFound, Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ErrorResponse { Code = ErrorCodes.ValidationFailed, Message = ex.Message });
            }
        }

        /// <summary>
        /// 編輯模式 - 整批更新投票名單
        /// </summary>
        [HttpPut("{id}/voters")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVoters(Guid id, [FromBody] UpdateEligibleVotersRequest request)
        {
            try
            {
                await _voteActivityService.UpdateEligibleVotersAsync(id, request);
                return Ok(new SuccessResponse { Success = true, Message = "投票名單已更新" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ErrorResponse { Code = ErrorCodes.NotFound, Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ErrorResponse { Code = ErrorCodes.ValidationFailed, Message = ex.Message });
            }
        }

        /// <summary>
        /// 下載投票名單 CSV 範本
        /// </summary>
        [HttpGet("voters/template")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult DownloadVotersTemplate()
        {
            // 加入 UTF-8 BOM 讓 Excel 正確開啟中文
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var csvContent = "工號,名稱,單位,生日\r\nA001,王小明,研發部,1990-01-01\r\nA002,陳小華,業務部,1992-05-20\r\n";
            var csvBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
            var bytes = bom.Concat(csvBytes).ToArray();
            return File(bytes, "text/csv; charset=utf-8", "voters_template.csv");
        }

        /// <summary>
        /// 解析 CSV 投票名單（純解析，不存 DB）
        /// 支援欄位：工號, 名稱, 單位, 生日
        /// 工號重複只保留第一筆，回傳 JSON 供前端暫存後連同活動一起儲存
        /// </summary>
        [HttpPost("voters/parse")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ParseVotersCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ErrorResponse { Code = ErrorCodes.ValidationFailed, Message = "請上傳 CSV 檔案" });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new ErrorResponse { Code = ErrorCodes.ValidationFailed, Message = "檔案大小不可超過 5MB" });

            var result = await ParseVotersCsvInternal(file);
            return Ok(result);
        }

        /// <summary>
        /// 上傳 CSV 匯入投票名單（保留舊路由相容性，內部共用解析邏輯）
        /// </summary>
        [HttpPost("{id}/voters/import")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportVotersCsv(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ErrorResponse { Code = ErrorCodes.ValidationFailed, Message = "請上傳 CSV 檔案" });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new ErrorResponse { Code = ErrorCodes.ValidationFailed, Message = "檔案大小不可超過 5MB" });

            var result = await ParseVotersCsvInternal(file);
            return Ok(result);
        }

        /// <summary>
        /// 共用 CSV 解析邏輯
        /// </summary>
        private static async Task<List<object>> ParseVotersCsvInternal(IFormFile file)
        {
            var result = new List<object>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var stream = file.OpenReadStream();
            using var reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            string? line;
            int lineNo = 0;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNo++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                // 跳過標頭行（第一欄含中文或英文標頭關鍵字）
                if (lineNo == 1)
                {
                    var firstField = ParseCsvLine(line).FirstOrDefault() ?? "";
                    if (firstField.Contains("工號") || firstField.ToLower().Contains("employee") || firstField.ToLower() == "id")
                        continue;
                }

                var fields = ParseCsvLine(line);
                if (fields.Count < 1) continue;

                var employeeNo = fields.Count > 0 ? fields[0].Trim() : "";
                var name       = fields.Count > 1 ? fields[1].Trim() : "";
                var unit       = fields.Count > 2 ? fields[2].Trim() : "";
                var birthday   = fields.Count > 3 ? fields[3].Trim() : "";

                if (string.IsNullOrEmpty(employeeNo)) continue;

                // 工號重複只保留第一筆
                if (!seen.Add(employeeNo)) continue;

                result.Add(new
                {
                    EmployeeNo = employeeNo,
                    Name       = name,
                    Department = unit,
                    BirthDate  = birthday
                });
            }

            return result;
        }

        /// <summary>
        /// 解析單行 CSV（支援帶引號的欄位）
        /// </summary>
        private static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var sb = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            fields.Add(sb.ToString());
            return fields;
        }

        /// <summary>
        /// 上傳候選人圖片
        /// </summary>
        [HttpPost("candidates/upload-image")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadCandidateImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ErrorResponse { Code = ErrorCodes.ValidationFailed, Message = "請選擇圖片檔案" });
            }

            // 驗證檔案類型
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new ErrorResponse { Code = ErrorCodes.ValidationFailed, Message = "僅支援 JPG、PNG、GIF、WebP 格式" });
            }

            // 限制 5MB
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new ErrorResponse { Code = ErrorCodes.ValidationFailed, Message = "圖片大小不可超過 5MB" });
            }

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "candidates");
            Directory.CreateDirectory(uploadsDir);

            var ext = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"/uploads/candidates/{fileName}";

            return Ok(new { imageUrl });
        }
    }
}
