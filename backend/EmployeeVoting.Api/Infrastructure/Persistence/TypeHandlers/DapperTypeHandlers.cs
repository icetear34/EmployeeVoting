using Dapper;
using System.Data;

namespace EmployeeVoting.Api.Infrastructure.Persistence.TypeHandlers
{
    /// <summary>
    /// Dapper Guid 型別處理器 - 處理 SQLite TEXT 與 C# Guid 的轉換
    /// </summary>
    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            if (value == null || value == DBNull.Value)
                return Guid.Empty;

            return Guid.Parse(value.ToString()!);
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToString();
            parameter.DbType = DbType.String;
        }
    }

    /// <summary>
    /// Dapper Nullable Guid 型別處理器
    /// </summary>
    public class NullableGuidTypeHandler : SqlMapper.TypeHandler<Guid?>
    {
        public override Guid? Parse(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            return Guid.Parse(value.ToString()!);
        }

        public override void SetValue(IDbDataParameter parameter, Guid? value)
        {
            parameter.Value = value?.ToString() ?? (object)DBNull.Value;
            parameter.DbType = DbType.String;
        }
    }

    /// <summary>
    /// Dapper DateTime 型別處理器 - 處理 SQLite TEXT (ISO8601) 與 C# DateTime 的轉換
    /// </summary>
    public class DateTimeTypeHandler : SqlMapper.TypeHandler<DateTime>
    {
        public override DateTime Parse(object value)
        {
            if (value == null || value == DBNull.Value)
                return DateTime.MinValue;

            return DateTime.Parse(value.ToString()!).ToUniversalTime();
        }

        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.Value = value.ToUniversalTime().ToString("O");
            parameter.DbType = DbType.String;
        }
    }

    /// <summary>
    /// Dapper Bool 型別處理器 - 處理 SQLite INTEGER 與 C# bool 的轉換
    /// </summary>
    public class BoolTypeHandler : SqlMapper.TypeHandler<bool>
    {
        public override bool Parse(object value)
        {
            if (value == null || value == DBNull.Value)
                return false;

            return Convert.ToInt64(value) == 1;
        }

        public override void SetValue(IDbDataParameter parameter, bool value)
        {
            parameter.Value = value ? 1 : 0;
            parameter.DbType = DbType.Int64;
        }
    }

    /// <summary>
    /// 註冊所有 TypeHandler
    /// </summary>
    public static class DapperTypeHandlers
    {
        private static bool _initialized = false;
        private static readonly object _lock = new object();

        public static void Initialize()
        {
            if (_initialized) return;

            lock (_lock)
            {
                if (_initialized) return;

                SqlMapper.AddTypeHandler(new GuidTypeHandler());
                SqlMapper.AddTypeHandler(new NullableGuidTypeHandler());
                SqlMapper.AddTypeHandler(new DateTimeTypeHandler());
                SqlMapper.AddTypeHandler(new BoolTypeHandler());

                _initialized = true;
            }
        }
    }
}
