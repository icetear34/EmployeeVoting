using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Common;
using EmployeeVoting.Api.Dtos.Common;
using EmployeeVoting.Api.Dtos.Employee;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeVoting.Api.Controllers
{
    /// <summary>
    /// 員工認證 API
    /// </summary>
    [ApiController]
    [Route("api/employee-auth")]
    public class EmployeeAuthController : ControllerBase
    {
        private readonly IEmployeeAuthService _employeeAuthService;
        private readonly ICaptchaService _captchaService;

        public EmployeeAuthController(
            IEmployeeAuthService employeeAuthService,
            ICaptchaService captchaService)
        {
            _employeeAuthService = employeeAuthService;
            _captchaService = captchaService;
        }

        /// <summary>
        /// 取得驗證碼（員工登入用）
        /// </summary>
        [HttpGet("captcha")]
        [ProducesResponseType(typeof(CaptchaResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCaptcha()
        {
            var result = await _captchaService.GenerateAsync(CaptchaPurpose.EmployeeLogin);
            return Ok(result);
        }

        /// <summary>
        /// 員工登入
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(EmployeeLoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] EmployeeLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EmployeeNo))
                return BadRequest(new ErrorResponse { Code = ErrorCodes.ValidationFailed, Message = "工號不可為空" });

            if (string.IsNullOrWhiteSpace(request.BirthDate))
                return BadRequest(new ErrorResponse { Code = ErrorCodes.ValidationFailed, Message = "生日不可為空" });

            if (string.IsNullOrWhiteSpace(request.CaptchaId) || string.IsNullOrWhiteSpace(request.CaptchaCode))
                return BadRequest(new ErrorResponse { Code = ErrorCodes.ValidationFailed, Message = "驗證碼不可為空" });

            var (response, errorCode, errorMessage) = await _employeeAuthService.LoginAsync(request);

            if (response == null)
            {
                var statusCode = errorCode switch
                {
                    ErrorCodes.InvalidCaptcha or ErrorCodes.CaptchaExpired => StatusCodes.Status400BadRequest,
                    ErrorCodes.ActivityNotActive => StatusCodes.Status403Forbidden,
                    _ => StatusCodes.Status401Unauthorized
                };
                return StatusCode(statusCode, new ErrorResponse
                {
                    Code = errorCode ?? ErrorCodes.InternalError,
                    Message = errorMessage ?? "登入失敗"
                });
            }

            // 設定 HttpOnly Cookie
            Response.Cookies.Append(CookieNames.EmployeeSession, response.SessionToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/"
            });
            // 前端可讀標記
            Response.Cookies.Append("employee_logged_in", "1", new CookieOptions
            {
                HttpOnly = false,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/"
            });

            return Ok(response);
        }

        /// <summary>
        /// 取得目前登入員工資訊
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(EmployeeMeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMe()
        {
            var sessionToken = GetSessionToken();

            if (string.IsNullOrWhiteSpace(sessionToken))
                return Unauthorized(new ErrorResponse { Code = ErrorCodes.Unauthorized, Message = "未登入" });

            var (response, errorCode) = await _employeeAuthService.GetCurrentUserAsync(sessionToken);

            if (response == null)
                return Unauthorized(new ErrorResponse
                {
                    Code = errorCode ?? ErrorCodes.Unauthorized,
                    Message = "登入已過期"
                });

            return Ok(response);
        }

        /// <summary>
        /// 員工登出
        /// </summary>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout()
        {
            var sessionToken = GetSessionToken();

            if (!string.IsNullOrWhiteSpace(sessionToken))
                await _employeeAuthService.LogoutAsync(sessionToken);

            Response.Cookies.Delete(CookieNames.EmployeeSession);
            Response.Cookies.Delete("employee_logged_in");

            return NoContent();
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
