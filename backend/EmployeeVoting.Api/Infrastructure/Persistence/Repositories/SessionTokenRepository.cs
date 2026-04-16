using Dapper;
using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Domain.Entities;

namespace EmployeeVoting.Api.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Session Token Repository 實作
    /// </summary>
    public class SessionTokenRepository : ISessionTokenRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SessionTokenRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<SessionToken?> GetByTokenAsync(string token)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<SessionToken>(
                @"SELECT Id, Token, Role, EmployeeNo, AdminUserId, CurrentVoteActivityId, 
                         ExpireAt, CreatedAt, IsRevoked 
                  FROM SessionToken 
                  WHERE Token = @Token AND IsRevoked = 0",
                new { Token = token });
        }

        public async Task<string> CreateAsync(SessionToken session)
        {
            using var connection = _connectionFactory.CreateConnection();
            session.Id = Guid.NewGuid();
            session.Token = GenerateToken();
            session.CreatedAt = DateTime.UtcNow;
            session.IsRevoked = false;

            await connection.ExecuteAsync(
                @"INSERT INTO SessionToken (Id, Token, Role, EmployeeNo, AdminUserId, 
                                            CurrentVoteActivityId, ExpireAt, CreatedAt, IsRevoked)
                  VALUES (@Id, @Token, @Role, @EmployeeNo, @AdminUserId, 
                          @CurrentVoteActivityId, @ExpireAt, @CreatedAt, @IsRevoked)",
                session);

            return session.Token;
        }

        public async Task RevokeAsync(string token)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                @"UPDATE SessionToken SET IsRevoked = 1 WHERE Token = @Token",
                new { Token = token });
        }

        public async Task RevokeAllByAdminUserIdAsync(Guid adminUserId)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                @"UPDATE SessionToken SET IsRevoked = 1 WHERE AdminUserId = @AdminUserId",
                new { AdminUserId = adminUserId });
        }

        public async Task RevokeAllByEmployeeNoAsync(string employeeNo)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                @"UPDATE SessionToken SET IsRevoked = 1 WHERE EmployeeNo = @EmployeeNo",
                new { EmployeeNo = employeeNo });
        }

        public async Task CleanupExpiredAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                @"DELETE FROM SessionToken WHERE ExpireAt < @Now OR IsRevoked = 1",
                new { Now = DateTime.UtcNow });
        }

        private static string GenerateToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .TrimEnd('=')
                + Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .TrimEnd('=');
        }
    }
}
