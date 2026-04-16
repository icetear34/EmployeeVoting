using Microsoft.Data.Sqlite;
using System.Data;

namespace EmployeeVoting.Api.Infrastructure.Persistence
{
    /// <summary>
    /// 資料庫連線工廠介面
    /// </summary>
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    /// <summary>
    /// SQLite 資料庫連線工廠實作
    /// </summary>
    public class SqliteConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqliteConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }
    }
}
