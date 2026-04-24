using Dapper;
using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Domain.Entities;

namespace EmployeeVoting.Api.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// 驗證碼 Session Repository 實作
    /// </summary>
    public class CaptchaSessionRepository : ICaptchaSessionRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CaptchaSessionRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<CaptchaSession?> GetByCaptchaIdAsync(string captchaId)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<CaptchaSession>(
                @"SELECT Id, CaptchaId, Code, Purpose, ExpireAt, IsUsed, CreatedAt 
                  FROM CaptchaSession 
                  WHERE CaptchaId = @CaptchaId",
                new { CaptchaId = captchaId });
        }

        public async Task<string> CreateAsync(CaptchaSession captchaSession)
        {
            using var connection = _connectionFactory.CreateConnection();
            captchaSession.Id = Guid.NewGuid();
            captchaSession.CaptchaId = Guid.NewGuid().ToString("N");
            captchaSession.CreatedAt = DateTime.Now;
            captchaSession.IsUsed = false;

            await connection.ExecuteAsync(
                @"INSERT INTO CaptchaSession (Id, CaptchaId, Code, Purpose, ExpireAt, IsUsed, CreatedAt)
                  VALUES (@Id, @CaptchaId, @Code, @Purpose, @ExpireAt, @IsUsed, @CreatedAt)",
                captchaSession);

            return captchaSession.CaptchaId;
        }

        public async Task MarkAsUsedAsync(string captchaId)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                @"UPDATE CaptchaSession SET IsUsed = 1 WHERE CaptchaId = @CaptchaId",
                new { CaptchaId = captchaId });
        }

        public async Task CleanupExpiredAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                @"DELETE FROM CaptchaSession WHERE ExpireAt < @Now OR IsUsed = 1",
                new { Now = DateTime.Now });
        }
    }
}
