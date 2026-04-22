using Dapper;
using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Domain.Entities;

namespace EmployeeVoting.Api.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// 投票紀錄 Repository 實作
    /// </summary>
    public class VoteRecordRepository : IVoteRecordRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public VoteRecordRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> HasVotedAsync(Guid activityId, string employeeNo)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT COUNT(1) FROM VoteRecord
                WHERE VoteActivityId = @VoteActivityId AND EmployeeNo = @EmployeeNo";

            var count = await connection.ExecuteScalarAsync<int>(sql,
                new { VoteActivityId = activityId, EmployeeNo = employeeNo });

            return count > 0;
        }

        public async Task<IEnumerable<VoteRecord>> GetByActivityIdAsync(Guid activityId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT Id, VoteActivityId, CandidateId, EmployeeNo, VotedAt, ClientIp, UserAgent
                FROM VoteRecord
                WHERE VoteActivityId = @VoteActivityId";

            return await connection.QueryAsync<VoteRecord>(sql, new { VoteActivityId = activityId });
        }

        public async Task<int> GetCountByCandidateIdAsync(Guid candidateId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = "SELECT COUNT(1) FROM VoteRecord WHERE CandidateId = @CandidateId";
            return await connection.ExecuteScalarAsync<int>(sql, new { CandidateId = candidateId });
        }

        public async Task CreateAsync(VoteRecord record)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                INSERT INTO VoteRecord (Id, VoteActivityId, CandidateId, EmployeeNo, VotedAt, ClientIp, UserAgent)
                VALUES (@Id, @VoteActivityId, @CandidateId, @EmployeeNo, @VotedAt, @ClientIp, @UserAgent)";

            await connection.ExecuteAsync(sql, record);
        }

        public async Task BatchCreateAsync(IEnumerable<VoteRecord> records)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            const string sql = @"
                INSERT INTO VoteRecord (Id, VoteActivityId, CandidateId, EmployeeNo, VotedAt, ClientIp, UserAgent)
                VALUES (@Id, @VoteActivityId, @CandidateId, @EmployeeNo, @VotedAt, @ClientIp, @UserAgent)";

            foreach (var record in records)
            {
                await connection.ExecuteAsync(sql, record, transaction);
            }

            transaction.Commit();
        }
    }
}
