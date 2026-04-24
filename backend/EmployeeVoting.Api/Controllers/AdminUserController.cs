using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Domain.Exceptions;
using EmployeeVoting.Api.Dtos.Admin;
using EmployeeVoting.Api.Dtos.Common;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeVoting.Api.Controllers
{
    /// <summary>
    /// 管理者帳號 CRUD API
    /// </summary>
    [Route("api/admin/users")]
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class AdminUserController : AdminBaseController
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUserController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        /// <summary>
        /// 取得管理者帳號列表（分頁 + 關鍵字搜尋）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? keyword,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _adminUserService.GetPagedAsync(new AdminUserQueryRequest
            {
                Keyword = keyword,
                Page = page,
                PageSize = pageSize
            });

            return Ok(result);
        }

        /// <summary>
        /// 取得單一管理者帳號
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _adminUserService.GetByIdAsync(id);
            if (user == null)
                return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = "帳號不存在" });

            return Ok(user);
        }

        /// <summary>
        /// 新增管理者帳號
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateAdminUserRequest request)
        {
            try
            {
                var result = await _adminUserService.CreateAsync(request);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ErrorResponse { Code = ex.ErrorCode, Message = ex.Message });
            }
        }

        /// <summary>
        /// 更新管理者帳號
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateAdminUserRequest request)
        {
            try
            {
                var result = await _adminUserService.UpdateAsync(id, request);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ErrorResponse { Code = ex.ErrorCode, Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ErrorResponse { Code = ex.ErrorCode, Message = ex.Message });
            }
        }

        /// <summary>
        /// 重設管理者密碼
        /// </summary>
        [HttpPost("{id:guid}/reset-password")]
        public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetAdminPasswordRequest request)
        {
            try
            {
                await _adminUserService.ResetPasswordAsync(id, request);
                return Ok(new { message = "密碼重設成功" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ErrorResponse { Code = ex.ErrorCode, Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ErrorResponse { Code = ex.ErrorCode, Message = ex.Message });
            }
        }

        /// <summary>
        /// 刪除管理者帳號
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                await _adminUserService.DeleteAsync(id);
                return Ok(new { message = "刪除成功" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ErrorResponse { Code = ex.ErrorCode, Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ErrorResponse { Code = ex.ErrorCode, Message = ex.Message });
            }
        }
    }
}
