using Dapper;
using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Domain.Entities;

namespace EmployeeVoting.Api.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// 活動分區 Repository 實作
    /// </summary>
    public class ActivityGroupRepository : IActivityGroupRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ActivityGroupRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<ActivityGroup>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT Id, Name, Description, CreatedAt, CreatedBy, IsDeleted
                FROM ActivityGroup
                WHERE IsDeleted = 0
                ORDER BY CreatedAt ASC";
            return await connection.QueryAsync<ActivityGroup>(sql);
        }

        public async Task<ActivityGroup?> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT Id, Name, Description, CreatedAt, CreatedBy, IsDeleted
                FROM ActivityGroup
                WHERE Id = @Id AND IsDeleted = 0";
            return await connection.QueryFirstOrDefaultAsync<ActivityGroup>(sql, new { Id = id });
        }

        public async Task<Guid> CreateAsync(ActivityGroup group)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO ActivityGroup (Id, Name, Description, CreatedAt, CreatedBy, IsDeleted)
                VALUES (@Id, @Name, @Description, @CreatedAt, @CreatedBy, @IsDeleted)";
            await connection.ExecuteAsync(sql, group);
            return group.Id;
        }

        public async Task UpdateAsync(ActivityGroup group)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE ActivityGroup SET Name = @Name, Description = @Description
                WHERE Id = @Id";
            await connection.ExecuteAsync(sql, group);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "UPDATE ActivityGroup SET IsDeleted = 1 WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<Guid>> GetGroupIdsByAdminUserIdAsync(Guid adminUserId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT ag.ActivityGroupId
                FROM AdminUserGroup ag
                INNER JOIN ActivityGroup g ON g.Id = ag.ActivityGroupId
                WHERE ag.AdminUserId = @AdminUserId AND g.IsDeleted = 0";
            return await connection.QueryAsync<Guid>(sql, new { AdminUserId = adminUserId });
        }
    }
}
