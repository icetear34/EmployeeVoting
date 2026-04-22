using Dapper;
using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Domain.Entities;

namespace EmployeeVoting.Api.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// 可投票名單 Repository 實作
    /// </summary>
    public class EligibleVoterRepository : IEligibleVoterRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public EligibleVoterRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<EligibleVoter>> GetByActivityIdAsync(Guid activityId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT Id, VoteActivityId, EmployeeNo, Name, Department, BirthDate, CreatedAt
                FROM EligibleVoter
                WHERE VoteActivityId = @VoteActivityId
                ORDER BY CreatedAt ASC";

            return await connection.QueryAsync<EligibleVoter>(sql, new { VoteActivityId = activityId });
        }

        public async Task<int> GetCountByActivityIdAsync(Guid activityId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = "SELECT COUNT(1) FROM EligibleVoter WHERE VoteActivityId = @VoteActivityId";
            return await connection.ExecuteScalarAsync<int>(sql, new { VoteActivityId = activityId });
        }

        public async Task<EligibleVoter?> FindAsync(Guid activityId, string employeeNo)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT Id, VoteActivityId, EmployeeNo, Name, Department, BirthDate, CreatedAt
                FROM EligibleVoter
                WHERE VoteActivityId = @VoteActivityId AND EmployeeNo = @EmployeeNo";

            return await connection.QueryFirstOrDefaultAsync<EligibleVoter>(sql,
                new { VoteActivityId = activityId, EmployeeNo = employeeNo });
        }

        public async Task BatchCreateAsync(IEnumerable<EligibleVoter> voters)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            const string sql = @"
                INSERT INTO EligibleVoter (Id, VoteActivityId, EmployeeNo, Name, Department, BirthDate, CreatedAt)
                VALUES (@Id, @VoteActivityId, @EmployeeNo, @Name, @Department, @BirthDate, @CreatedAt)";

            foreach (var voter in voters)
            {
                await connection.ExecuteAsync(sql, voter, transaction);
            }

            transaction.Commit();
        }

        public async Task DeleteByActivityIdAsync(Guid activityId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = "DELETE FROM EligibleVoter WHERE VoteActivityId = @VoteActivityId";
            await connection.ExecuteAsync(sql, new { VoteActivityId = activityId });
        }

        public async Task ReplaceAllAsync(Guid activityId, IEnumerable<EligibleVoter> voters)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(
                "DELETE FROM EligibleVoter WHERE VoteActivityId = @VoteActivityId",
                new { VoteActivityId = activityId },
                transaction);

            const string insertSql = @"
                INSERT INTO EligibleVoter (Id, VoteActivityId, EmployeeNo, Name, Department, BirthDate, CreatedAt)
                VALUES (@Id, @VoteActivityId, @EmployeeNo, @Name, @Department, @BirthDate, @CreatedAt)";

            foreach (var voter in voters)
            {
                await connection.ExecuteAsync(insertSql, voter, transaction);
            }

            transaction.Commit();
        }

        public async Task<IEnumerable<EligibleVoter>> GetByEmployeeNoAsync(string employeeNo)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT Id, VoteActivityId, EmployeeNo, Name, Department, BirthDate, CreatedAt
                FROM EligibleVoter
                WHERE EmployeeNo = @EmployeeNo";

            return await connection.QueryAsync<EligibleVoter>(sql, new { EmployeeNo = employeeNo });
        }
    }
}
