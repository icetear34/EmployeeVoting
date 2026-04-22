using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Common;
using EmployeeVoting.Api.Dtos.Common;
using EmployeeVoting.Api.Dtos.Employee;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeVoting.Api.Controllers
{
    /// <summary>
    /// 員工投票 API
    /// </summary>
    [ApiController]
    [Route("api/employee-vote")]
    public class EmployeeVoteController : ControllerBase
    {
        private readonly IEmployeeAuthService _employeeAuthService;
        private readonly IEmployeeVoteService _employeeVoteService;

        public EmployeeVoteController(
            IEmployeeAuthService employeeAuthService,
            IEmployeeVoteService employeeVoteService)
        {
            _employeeAuthService = employeeAuthService;
            _employeeVoteService = employeeVoteService;
        }

        /// <summary>
        /// 取得員工可參與的活動列表（含候選人）
        /// </summary>
        [HttpGet("activities")]
        [ProducesResponseType(typeof(List<ActivityWithCandidatesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetActivities()
        {
            var (isValid, session, errorResponse) = await AuthorizeEmployeeAsync();
            if (!isValid) return errorResponse!;

            var activities = await _employeeVoteService.GetActivitiesForEmployeeAsync(session!.EmployeeNo!);
            return Ok(activities);
        }

        /// <summary>
        /// 批次送出投票
        /// </summary>
        [HttpPost("submit-batch")]
        [ProducesResponseType(typeof(VoteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SubmitBatch([FromBody] SubmitBatchVoteRequest request)
        {
            var (isValid, session, errorResponse) = await AuthorizeEmployeeAsync();
            if (!isValid) return errorResponse!;

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            var userAgent = Request.Headers.UserAgent.FirstOrDefault() ?? "";

            var (response, errorCode) = await _employeeVoteService.SubmitBatchVoteAsync(
                session!.EmployeeNo!, request, clientIp, userAgent);

            if (!response.Success)
            {
                var statusCode = errorCode switch
                {
                    ErrorCodes.AlreadyVoted => StatusCodes.Status409Conflict,
                    ErrorCodes.ActivityNotActive or ErrorCodes.NotInVoterList or ErrorCodes.InvalidCandidate
                        => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status400BadRequest
                };
                return StatusCode(statusCode, new ErrorResponse
                {
                    Code = errorCode ?? ErrorCodes.BatchVoteFailed,
                    Message = response.Message
                });
            }

            return Ok(response);
        }

        /// <summary>
        /// 取得活動開票結果（長條圖）
        /// </summary>
        [HttpGet("activities/{activityId:guid}/result-bars")]
        [ProducesResponseType(typeof(ActivityResultBarsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetResultBars(Guid activityId)
        {
            var (isValid, session, errorResponse) = await AuthorizeEmployeeAsync();
            if (!isValid) return errorResponse!;

            var result = await _employeeVoteService.GetResultBarsAsync(activityId, session!.EmployeeNo!);

            if (result == null)
                return Forbid();

            return Ok(result);
        }

        // --- 私有輔助方法 ---

        private async Task<(bool isValid, Domain.Entities.SessionToken? session, IActionResult? errorResponse)> AuthorizeEmployeeAsync()
        {
            var sessionToken = GetSessionToken();

            if (string.IsNullOrWhiteSpace(sessionToken))
            {
                return (false, null, Unauthorized(new ErrorResponse
                {
                    Code = ErrorCodes.Unauthorized,
                    Message = "未登入"
                }));
            }

            var (isValid, session) = await _employeeAuthService.ValidateSessionAsync(sessionToken);

            if (!isValid || session == null)
            {
                return (false, null, Unauthorized(new ErrorResponse
                {
                    Code = ErrorCodes.SessionExpired,
                    Message = "登入已過期"
                }));
            }

            return (true, session, null);
        }

        private string? GetSessionToken()
        {
            if (Request.Cookies.TryGetValue(CookieNames.EmployeeSession, out var cookieToken))
                return cookieToken;

            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
                return authHeader.Substring("Bearer ".Length);

            return null;
        }
    }
}
