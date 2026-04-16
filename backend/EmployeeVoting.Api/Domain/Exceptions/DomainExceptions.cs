namespace EmployeeVoting.Api.Domain.Exceptions
{
    /// <summary>
    /// 領域例外基底類別
    /// </summary>
    public abstract class DomainException : Exception
    {
        public string ErrorCode { get; }

        protected DomainException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    /// <summary>
    /// 驗證失敗例外
    /// </summary>
    public class ValidationException : DomainException
    {
        public ValidationException(string message) 
            : base("VALIDATION_FAILED", message) { }
    }

    /// <summary>
    /// 未授權例外
    /// </summary>
    public class UnauthorizedException : DomainException
    {
        public UnauthorizedException(string message = "未授權的存取") 
            : base("UNAUTHORIZED", message) { }
    }

    /// <summary>
    /// 認證失敗例外
    /// </summary>
    public class AuthenticationException : DomainException
    {
        public AuthenticationException(string errorCode, string message) 
            : base(errorCode, message) { }
    }

    /// <summary>
    /// 找不到資源例外
    /// </summary>
    public class NotFoundException : DomainException
    {
        public NotFoundException(string message = "找不到資源") 
            : base("NOT_FOUND", message) { }
    }

    /// <summary>
    /// 業務規則違反例外
    /// </summary>
    public class BusinessRuleException : DomainException
    {
        public BusinessRuleException(string errorCode, string message) 
            : base(errorCode, message) { }
    }
}
