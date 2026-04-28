using Dapper;
using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Domain.Entities;
using EmployeeVoting.Api.Dtos.Admin;

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
                SELECT Id, Name, Description, StartTime, EndTime, 
                       CreatedAt, CreatedBy, IsDeleted,
                       IsResultViewable, ResultViewStartTime, ResultViewEndTime, ActivityGroupId
                FROM VoteActivity 
                WHERE Id = @Id";
            
            return await connection.QueryFirstOrDefaultAsync<VoteActivity>(sql, new { Id = id });
        }

        /// <inheritdoc/>
        /// <inheritdoc/>
        public async Task<IEnumerable<VoteActivity>> GetAllAsync(bool includeDeleted = false)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT Id, Name, Description, StartTime, EndTime, 
                       CreatedAt, CreatedBy, IsDeleted,
                       IsResultViewable, ResultViewStartTime, ResultViewEndTime, ActivityGroupId
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
                SELECT Id, Name, Description, StartTime, EndTime, 
                       CreatedAt, CreatedBy, IsDeleted,
                       IsResultViewable, ResultViewStartTime, ResultViewEndTime, ActivityGroupId
                FROM VoteActivity 
                WHERE IsDeleted = 0 
                  AND datetime('now','localtime') >= datetime(StartTime) 
                  AND datetime('now','localtime') <= datetime(EndTime)
                ORDER BY StartTime ASC";
            
            return await connection.QueryAsync<VoteActivity>(sql);
        }

        /// <inheritdoc/>
        public async Task<Guid> CreateAsync(VoteActivity activity)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                INSERT INTO VoteActivity 
                    (Id, Name, Description, StartTime, EndTime, CreatedAt, CreatedBy, IsDeleted,
                     IsResultViewable, ResultViewStartTime, ResultViewEndTime, ActivityGroupId)
                VALUES 
                    (@Id, @Name, @Description, @StartTime, @EndTime, @CreatedAt, @CreatedBy, @IsDeleted,
                     @IsResultViewable, @ResultViewStartTime, @ResultViewEndTime, @ActivityGroupId)";
            
            await connection.ExecuteAsync(sql, activity);
            
            return activity.Id;
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(VoteActivity activity)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE VoteActivity 
                SET Name                 = @Name, 
                    Description          = @Description, 
                    StartTime            = @StartTime, 
                    EndTime              = @EndTime,
                    IsResultViewable     = @IsResultViewable,
                    ResultViewStartTime  = @ResultViewStartTime,
                    ResultViewEndTime    = @ResultViewEndTime
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
        public async Task<(IEnumerable<VoteActivity> Items, int TotalCount)> GetPagedAsync(ActivityQueryRequest query, IEnumerable<Guid>? groupFilter = null)
        {
            using var connection = _connectionFactory.CreateConnection();

            // 正規化參數
            var page     = Math.Max(1, query.Page);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);
            var keyword  = query.Keyword?.Trim();

            // 排序白名單
            var sortCol = (query.SortBy?.ToLower()) switch
            {
                "starttime" => "StartTime",
                "endtime"   => "EndTime",
                "name"      => "Name",
                _           => "CreatedAt"
            };
            var sortDir = query.SortDir?.ToLower() == "asc" ? "ASC" : "DESC";

            // 狀態轉為時間條件（SQLite 以 datetime() 比較）
            var statusWhere = (query.Status?.ToLower()) switch
            {
                "pending" => "AND datetime('now','localtime') < datetime(StartTime)",
                "active"  => "AND datetime('now','localtime') >= datetime(StartTime) AND datetime('now','localtime') <= datetime(EndTime)",
                "ended"   => "AND datetime('now','localtime') > datetime(EndTime)",
                _         => ""
            };

            var keywordWhere = string.IsNullOrEmpty(keyword) ? "" : "AND Name LIKE @Keyword";

            // 分區過濾（admin 只能看自己分區的活動）
            var groupFilterList = groupFilter?.ToList();
            string groupWhere;
            if (groupFilterList != null)
            {
                if (groupFilterList.Count == 0)
                {
                    // 沒有任何分區，回傳空結果
                    return (Enumerable.Empty<VoteActivity>(), 0);
                }
                var inClause = string.Join(",", groupFilterList.Select(g => $"'{g}'"));
                groupWhere = $"AND ActivityGroupId IN ({inClause})";
            }
            else
            {
                groupWhere = "";
            }

            var baseWhere = $"WHERE IsDeleted = 0 {statusWhere} {keywordWhere} {groupWhere}";

            var countSql = $"SELECT COUNT(1) FROM VoteActivity {baseWhere}";

            var dataSql = $@"
                SELECT Id, Name, Description, StartTime, EndTime,
                       CreatedAt, CreatedBy, IsDeleted,
                       IsResultViewable, ResultViewStartTime, ResultViewEndTime, ActivityGroupId
                FROM VoteActivity
                {baseWhere}
                ORDER BY {sortCol} {sortDir}
                LIMIT @PageSize OFFSET @Offset";

            var param = new
            {
                Keyword  = string.IsNullOrEmpty(keyword) ? null : $"%{keyword}%",
                PageSize = pageSize,
                Offset   = (page - 1) * pageSize
            };

            var total = await connection.ExecuteScalarAsync<int>(countSql, param);
            var items = await connection.QueryAsync<VoteActivity>(dataSql, param);

            return (items, total);
        }
    }
}
