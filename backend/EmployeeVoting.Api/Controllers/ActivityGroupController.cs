using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Common;
using EmployeeVoting.Api.Domain.Exceptions;
using EmployeeVoting.Api.Dtos.Admin;
using EmployeeVoting.Api.Dtos.Common;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeVoting.Api.Controllers
{
    /// <summary>
    /// 活動分區管理 API（僅 super_admin 可存取）
    /// </summary>
    [Route("api/admin/groups")]
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class ActivityGroupController : AdminBaseController
    {
        private readonly IActivityGroupService _groupService;

        public ActivityGroupController(IActivityGroupService groupService)
        {
            _groupService = groupService;
        }

        private IActionResult ForbiddenIfNotSuperAdmin()
        {
            if (CurrentSession?.Role != UserRoles.SuperAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Code = ErrorCodes.Forbidden,
                    Message = "僅超級管理員可執行此操作"
                });
            }
            return null!;
        }

        /// <summary>取得所有分區</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var forbidden = ForbiddenIfNotSuperAdmin();
            if (forbidden != null) return forbidden;

            var result = await _groupService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>取得分區詳情（含成員）</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var forbidden = ForbiddenIfNotSuperAdmin();
            if (forbidden != null) return forbidden;

            var result = await _groupService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new ErrorResponse { Code = ErrorCodes.NotFound, Message = "分區不存在" });

            return Ok(result);
        }

        /// <summary>建立分區</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateActivityGroupRequest request)
        {
            var forbidden = ForbiddenIfNotSuperAdmin();
            if (forbidden != null) return forbidden;

            try
            {
                var createdBy = CurrentSession?.AdminUserId.ToString() ?? "super_admin";
                var result = await _groupService.CreateAsync(request, createdBy);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ErrorResponse { Code = ErrorCodes.ValidationFailed, Message = ex.Message });
            }
        }

        /// <summary>更新分區</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateActivityGroupRequest request)
        {
            var forbidden = ForbiddenIfNotSuperAdmin();
            if (forbidden != null) return forbidden;

            try
            {
                var result = await _groupService.UpdateAsync(id, request);
                return Ok(result);
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

        /// <summary>刪除分區</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var forbidden = ForbiddenIfNotSuperAdmin();
            if (forbidden != null) return forbidden;

            try
            {
                await _groupService.DeleteAsync(id);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ErrorResponse { Code = ErrorCodes.NotFound, Message = ex.Message });
            }
        }

        /// <summary>設定分區成員</summary>
        [HttpPut("{id}/members")]
        public async Task<IActionResult> SetMembers(Guid id, [FromBody] SetGroupMembersRequest request)
        {
            var forbidden = ForbiddenIfNotSuperAdmin();
            if (forbidden != null) return forbidden;

            try
            {
                await _groupService.SetMembersAsync(id, request.AdminUserIds);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ErrorResponse { Code = ErrorCodes.NotFound, Message = ex.Message });
            }
        }
    }
}
