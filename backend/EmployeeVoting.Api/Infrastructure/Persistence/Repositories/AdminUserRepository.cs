using Dapper;
using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Domain.Entities;

namespace EmployeeVoting.Api.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// 管理者帳號 Repository 實作
    /// </summary>
    public class AdminUserRepository : IAdminUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AdminUserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<AdminUser?> GetByAccountAsync(string account)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
        SELECT
            Id,
            Account,
            Password,
            DisplayName,
            IsEnabled,
            CreatedAt,
            UpdatedAt
        FROM AdminUser
        WHERE Account = @Account";

            return await connection.QueryFirstOrDefaultAsync<AdminUser>(
                sql,
                new { Account = account });
        }

        public async Task<AdminUser?> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<AdminUser>(
                @"SELECT Id, Account, Password, DisplayName, IsEnabled, CreatedAt, UpdatedAt 
                  FROM AdminUser 
                  WHERE Id = @Id",
                new { Id = id.ToString("D") });
        }

        public async Task<IEnumerable<AdminUser>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<AdminUser>(
                @"SELECT Id, Account, Password, DisplayName, IsEnabled, CreatedAt, UpdatedAt 
                  FROM AdminUser 
                  ORDER BY CreatedAt DESC");
        }

        public async Task<Guid> CreateAsync(AdminUser adminUser)
        {
            using var connection = _connectionFactory.CreateConnection();
            adminUser.Id = Guid.NewGuid();
            adminUser.CreatedAt = DateTime.Now;
            adminUser.UpdatedAt = DateTime.Now;

            await connection.ExecuteAsync(
                @"INSERT INTO AdminUser (Id, Account, Password, DisplayName, IsEnabled, CreatedAt, UpdatedAt)
                  VALUES (@Id, @Account, @Password, @DisplayName, @IsEnabled, @CreatedAt, @UpdatedAt)",
                adminUser);

            return adminUser.Id;
        }

        public async Task UpdateAsync(AdminUser adminUser)
        {
            using var connection = _connectionFactory.CreateConnection();
            adminUser.UpdatedAt = DateTime.Now;

            await connection.ExecuteAsync(
                @"UPDATE AdminUser 
                  SET Account = @Account, 
                      Password = @Password, 
                      DisplayName = @DisplayName, 
                      IsEnabled = @IsEnabled, 
                      UpdatedAt = @UpdatedAt
                  WHERE Id = @Id",
                adminUser);
        }

        public async Task<bool> AccountExistsAsync(string account, Guid? excludeId = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT COUNT(*) FROM AdminUser WHERE Account = @Account";

            if (excludeId.HasValue)
            {
                sql += " AND Id != @ExcludeId";
                return await connection.ExecuteScalarAsync<int>(sql,
                    new { Account = account, ExcludeId = excludeId.Value }) > 0;
            }

            return await connection.ExecuteScalarAsync<int>(sql, new { Account = account }) > 0;
        }
    }
}
