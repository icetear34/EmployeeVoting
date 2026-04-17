using Dapper;
using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Domain.Entities;

namespace EmployeeVoting.Api.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// 投票活動 Repository 實作
    /// </summary>
    public class VoteActivityRepository : IVoteActivityRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public VoteActivityRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <inheritdoc/>
        public async Task<VoteActivity?> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT Id, ActivityCode, Name, Description, StartTime, EndTime, 
                       CreatedAt, CreatedBy, IsDeleted
                FROM VoteActivity 
                WHERE Id = @Id";
            
            return await connection.QueryFirstOrDefaultAsync<VoteActivity>(sql, new { Id = id });
        }

        /// <inheritdoc/>
        public async Task<VoteActivity?> GetByActivityCodeAsync(string activityCode)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT Id, ActivityCode, Name, Description, StartTime, EndTime, 
                       CreatedAt, CreatedBy, IsDeleted
                FROM VoteActivity 
                WHERE ActivityCode = @ActivityCode AND IsDeleted = 0";
            
            return await connection.QueryFirstOrDefaultAsync<VoteActivity>(sql, new { ActivityCode = activityCode });
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VoteActivity>> GetAllAsync(bool includeDeleted = false)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT Id, ActivityCode, Name, Description, StartTime, EndTime, 
                       CreatedAt, CreatedBy, IsDeleted
                FROM VoteActivity";
            
            if (!includeDeleted)
            {
                sql += " WHERE IsDeleted = 0";
            }
            
            sql += " ORDER BY CreatedAt DESC";
            
            return await connection.QueryAsync<VoteActivity>(sql);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VoteActivity>> GetActiveActivitiesAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT Id, ActivityCode, Name, Description, StartTime, EndTime, 
                       CreatedAt, CreatedBy, IsDeleted
                FROM VoteActivity 
                WHERE IsDeleted = 0 
                  AND datetime('now') >= datetime(StartTime) 
                  AND datetime('now') <= datetime(EndTime)
                ORDER BY StartTime ASC";
            
            return await connection.QueryAsync<VoteActivity>(sql);
        }

        /// <inheritdoc/>
        public async Task<Guid> CreateAsync(VoteActivity activity)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                INSERT INTO VoteActivity 
                    (Id, ActivityCode, Name, Description, StartTime, EndTime, CreatedAt, CreatedBy, IsDeleted)
                VALUES 
                    (@Id, @ActivityCode, @Name, @Description, @StartTime, @EndTime, @CreatedAt, @CreatedBy, @IsDeleted)";
            
            await connection.ExecuteAsync(sql, activity);
            
            return activity.Id;
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(VoteActivity activity)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE VoteActivity 
                SET Name = @Name, 
                    Description = @Description, 
                    StartTime = @StartTime, 
                    EndTime = @EndTime
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, activity);
        }

        /// <inheritdoc/>
        public async Task SoftDeleteAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE VoteActivity 
                SET IsDeleted = 1 
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        /// <inheritdoc/>
        public async Task<bool> ActivityCodeExistsAsync(string activityCode, Guid? excludeId = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = "SELECT COUNT(1) FROM VoteActivity WHERE ActivityCode = @ActivityCode AND IsDeleted = 0";
            
            if (excludeId.HasValue)
            {
                sql += " AND Id != @ExcludeId";
            }
            
            var count = await connection.ExecuteScalarAsync<int>(sql, new { ActivityCode = activityCode, ExcludeId = excludeId });
            
            return count > 0;
        }

        /// <inheritdoc/>
        public async Task<string> GenerateActivityCodeAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // 產生格式：VOTE-YYYYMMDD-XXXX（流水號）
            var datePrefix = $"VOTE-{DateTime.UtcNow:yyyyMMdd}-";
            
            // 取得今天最大的流水號
            const string sql = @"
                SELECT ActivityCode 
                FROM VoteActivity 
                WHERE ActivityCode LIKE @Prefix || '%'
                ORDER BY ActivityCode DESC 
                LIMIT 1";
            
            var lastCode = await connection.QueryFirstOrDefaultAsync<string>(sql, new { Prefix = datePrefix });
            
            int nextNumber = 1;
            
            if (!string.IsNullOrEmpty(lastCode))
            {
                // 解析流水號
                var parts = lastCode.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
            
            return $"{datePrefix}{nextNumber:D4}";
        }
    }
}
