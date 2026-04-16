using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Common;
using EmployeeVoting.Api.Dtos.Admin;
using EmployeeVoting.Api.Dtos.Common;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeVoting.Api.Controllers
{
    /// <summary>
    /// 管理者認證 API
    /// </summary>
    [ApiController]
    [Route("api/admin-auth")]
    public class AdminAuthController : ControllerBase
    {
        private readonly IAdminAuthService _adminAuthService;
        private readonly ICaptchaService _captchaService;

        public AdminAuthController(
            IAdminAuthService adminAuthService,
            ICaptchaService captchaService)
        {
            _adminAuthService = adminAuthService;
            _captchaService = captchaService;
        }

        /// <summary>
        /// 取得驗證碼
        /// </summary>
        [HttpGet("captcha")]
        [ProducesResponseType(typeof(CaptchaResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCaptcha()
        {
            var result = await _captchaService.GenerateAsync(CaptchaPurpose.AdminLogin);
            return Ok(result);
        }

        /// <summary>
        /// 管理者登入
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AdminLoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
        {
            // 基本驗證
            if (string.IsNullOrWhiteSpace(request.Account))
            {
                return BadRequest(new ErrorResponse
                {
                    Code = ErrorCodes.ValidationFailed,
                    Message = "帳號不可為空"
                });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ErrorResponse
                {
                    Code = ErrorCodes.ValidationFailed,
                    Message = "密碼不可為空"
                });
            }

            if (string.IsNullOrWhiteSpace(request.CaptchaId) || string.IsNullOrWhiteSpace(request.CaptchaCode))
            {
                return BadRequest(new ErrorResponse
                {
                    Code = ErrorCodes.ValidationFailed,
                    Message = "驗證碼不可為空"
                });
            }

            var (response, errorCode, errorMessage) = await _adminAuthService.LoginAsync(request);

            if (response == null)
            {
                var statusCode = errorCode switch
                {
                    ErrorCodes.InvalidCaptcha or ErrorCodes.CaptchaExpired => StatusCodes.Status400BadRequest,
                    ErrorCodes.AccountDisabled => StatusCodes.Status403Forbidden,
                    _ => StatusCodes.Status401Unauthorized
                };

                return StatusCode(statusCode, new ErrorResponse
                {
                    Code = errorCode ?? ErrorCodes.InternalError,
                    Message = errorMessage ?? "登入失敗"
                });
            }

            // 設定 Cookie
            Response.Cookies.Append(CookieNames.AdminSession, response.SessionToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            return Ok(response);
        }

        /// <summary>
        /// 取得目前登入資訊
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(AdminMeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMe()
        {
            var sessionToken = GetSessionToken();

            if (string.IsNullOrWhiteSpace(sessionToken))
            {
                return Unauthorized(new ErrorResponse
                {
                    Code = ErrorCodes.Unauthorized,
                    Message = "未登入"
                });
            }

            var (response, errorCode) = await _adminAuthService.GetCurrentUserAsync(sessionToken);

            if (response == null)
            {
                return Unauthorized(new ErrorResponse
                {
                    Code = errorCode ?? ErrorCodes.Unauthorized,
                    Message = errorCode == ErrorCodes.AccountDisabled ? "此帳號已被停用" : "登入已過期"
                });
            }

            return Ok(response);
        }

        /// <summary>
        /// 登出
        /// </summary>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout()
        {
            var sessionToken = GetSessionToken();

            if (!string.IsNullOrWhiteSpace(sessionToken))
            {
                await _adminAuthService.LogoutAsync(sessionToken);
            }

            // 清除 Cookie
            Response.Cookies.Delete(CookieNames.AdminSession);

            return NoContent();
        }

        /// <summary>
        /// 從 Cookie 或 Header 取得 Session Token
        /// </summary>
        private string? GetSessionToken()
        {
            // 優先從 Cookie 取得
            if (Request.Cookies.TryGetValue(CookieNames.AdminSession, out var cookieToken))
            {
                return cookieToken;
            }

            // 其次從 Authorization Header 取得
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length);
            }

            return null;
        }
    }
}
