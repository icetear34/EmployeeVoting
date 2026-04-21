using Dapper;
using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Domain.Entities;

namespace EmployeeVoting.Api.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// 候選人 Repository 實作
    /// </summary>
    public class CandidateRepository : ICandidateRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CandidateRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Candidate>> GetByActivityIdAsync(Guid activityId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT Id, VoteActivityId, Name, Description, ImagePath, SortOrder, IsEnabled, CreatedAt
                FROM Candidate
                WHERE VoteActivityId = @VoteActivityId
                ORDER BY SortOrder ASC, CreatedAt ASC";

            return await connection.QueryAsync<Candidate>(sql, new { VoteActivityId = activityId });
        }

        public async Task<Candidate?> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT Id, VoteActivityId, Name, Description, ImagePath, SortOrder, IsEnabled, CreatedAt
                FROM Candidate
                WHERE Id = @Id";

            return await connection.QueryFirstOrDefaultAsync<Candidate>(sql, new { Id = id });
        }

        public async Task<Guid> CreateAsync(Candidate candidate)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                INSERT INTO Candidate (Id, VoteActivityId, Name, Description, ImagePath, SortOrder, IsEnabled, CreatedAt)
                VALUES (@Id, @VoteActivityId, @Name, @Description, @ImagePath, @SortOrder, @IsEnabled, @CreatedAt)";

            await connection.ExecuteAsync(sql, candidate);
            return candidate.Id;
        }

        public async Task BatchCreateAsync(IEnumerable<Candidate> candidates)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            const string sql = @"
                INSERT INTO Candidate (Id, VoteActivityId, Name, Description, ImagePath, SortOrder, IsEnabled, CreatedAt)
                VALUES (@Id, @VoteActivityId, @Name, @Description, @ImagePath, @SortOrder, @IsEnabled, @CreatedAt)";

            foreach (var candidate in candidates)
            {
                await connection.ExecuteAsync(sql, candidate, transaction);
            }

            transaction.Commit();
        }

        public async Task DeleteByActivityIdAsync(Guid activityId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = "DELETE FROM Candidate WHERE VoteActivityId = @VoteActivityId";
            await connection.ExecuteAsync(sql, new { VoteActivityId = activityId });
        }

        public async Task ReplaceAllAsync(Guid activityId, IEnumerable<Candidate> candidates)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            // 先刪除舊的
            await connection.ExecuteAsync(
                "DELETE FROM Candidate WHERE VoteActivityId = @VoteActivityId",
                new { VoteActivityId = activityId },
                transaction);

            // 再批次插入新的
            const string insertSql = @"
                INSERT INTO Candidate (Id, VoteActivityId, Name, Description, ImagePath, SortOrder, IsEnabled, CreatedAt)
                VALUES (@Id, @VoteActivityId, @Name, @Description, @ImagePath, @SortOrder, @IsEnabled, @CreatedAt)";

            foreach (var candidate in candidates)
            {
                await connection.ExecuteAsync(insertSql, candidate, transaction);
            }

            transaction.Commit();
        }
    }
}
