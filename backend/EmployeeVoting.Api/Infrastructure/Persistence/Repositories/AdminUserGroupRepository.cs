using Dapper;
using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Domain.Entities;

namespace EmployeeVoting.Api.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// 管理者分區對應 Repository 實作
    /// </summary>
    public class AdminUserGroupRepository : IAdminUserGroupRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AdminUserGroupRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<AdminUserGroup>> GetByAdminUserIdAsync(Guid adminUserId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT Id, AdminUserId, ActivityGroupId, CreatedAt
                FROM AdminUserGroup
                WHERE AdminUserId = @AdminUserId";
            return await connection.QueryAsync<AdminUserGroup>(sql, new { AdminUserId = adminUserId });
        }

        public async Task<IEnumerable<AdminUserGroup>> GetByGroupIdAsync(Guid groupId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT Id, AdminUserId, ActivityGroupId, CreatedAt
                FROM AdminUserGroup
                WHERE ActivityGroupId = @GroupId";
            return await connection.QueryAsync<AdminUserGroup>(sql, new { GroupId = groupId });
        }

        public async Task SetGroupsForAdminAsync(Guid adminUserId, IEnumerable<Guid> groupIds)
        {
            using var connection = _connectionFactory.CreateConnection();
            // 先刪除舊的關聯
            await connection.ExecuteAsync(
                "DELETE FROM AdminUserGroup WHERE AdminUserId = @AdminUserId",
                new { AdminUserId = adminUserId });

            // 插入新的關聯
            var now = DateTime.Now;
            foreach (var groupId in groupIds)
            {
                await connection.ExecuteAsync(
                    @"INSERT OR IGNORE INTO AdminUserGroup (Id, AdminUserId, ActivityGroupId, CreatedAt)
                      VALUES (@Id, @AdminUserId, @ActivityGroupId, @CreatedAt)",
                    new
                    {
                        Id = Guid.NewGuid(),
                        AdminUserId = adminUserId,
                        ActivityGroupId = groupId,
                        CreatedAt = now
                    });
            }
        }

        public async Task RemoveAllByGroupIdAsync(Guid groupId)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "DELETE FROM AdminUserGroup WHERE ActivityGroupId = @GroupId",
                new { GroupId = groupId });
        }
    }
}
