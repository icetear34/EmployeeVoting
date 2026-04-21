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
        /// 取得所有活動列表
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VoteActivityListItem>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActivities()
        {
            var activities = await _voteActivityService.GetAllActivitiesAsync();
            return Ok(activities);
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

                var activity = await _voteActivityService.CreateActivityAsync(request, createdBy);
                
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
