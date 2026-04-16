using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Common;
using EmployeeVoting.Api.Domain.Entities;
using EmployeeVoting.Api.Dtos.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EmployeeVoting.Api.Controllers
{
    /// <summary>
    /// 需要管理者認證的 Controller 基底類別
    /// </summary>
    [ApiController]
    public abstract class AdminBaseController : ControllerBase
    {
        /// <summary>
        /// 目前的 Session
        /// </summary>
        protected SessionToken? CurrentSession { get; private set; }

        /// <summary>
        /// 目前的管理者 Id
        /// </summary>
        protected Guid? CurrentAdminUserId => CurrentSession?.AdminUserId;

        /// <summary>
        /// 從 Cookie 或 Header 取得 Session Token
        /// </summary>
        internal string? GetSessionToken()
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

        /// <summary>
        /// 設定目前的 Session（由 Filter 使用）
        /// </summary>
        internal void SetCurrentSession(SessionToken session)
        {
            CurrentSession = session;
        }
    }

    /// <summary>
    /// 管理者認證過濾器
    /// </summary>
    public class AdminAuthFilter : IAsyncActionFilter
    {
        private readonly IAdminAuthService _adminAuthService;

        public AdminAuthFilter(IAdminAuthService adminAuthService)
        {
            _adminAuthService = adminAuthService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controller = context.Controller as AdminBaseController;
            if (controller == null)
            {
                await next();
                return;
            }

            var sessionToken = controller.GetSessionToken();

            if (string.IsNullOrWhiteSpace(sessionToken))
            {
                context.Result = new UnauthorizedObjectResult(new ErrorResponse
                {
                    Code = ErrorCodes.Unauthorized,
                    Message = "未登入"
                });
                return;
            }

            var (isValid, session) = await _adminAuthService.ValidateSessionAsync(sessionToken);

            if (!isValid || session == null)
            {
                context.Result = new UnauthorizedObjectResult(new ErrorResponse
                {
                    Code = ErrorCodes.SessionExpired,
                    Message = "登入已過期，請重新登入"
                });
                return;
            }

            controller.SetCurrentSession(session);
            await next();
        }
    }

    /// <summary>
    /// 套用管理者認證的 Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminAuthAttribute : TypeFilterAttribute
    {
        public AdminAuthAttribute() : base(typeof(AdminAuthFilter))
        {
        }
    }
}
