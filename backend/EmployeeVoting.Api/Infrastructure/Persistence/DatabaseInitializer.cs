using Dapper;

namespace EmployeeVoting.Api.Infrastructure.Persistence
{
    /// <summary>
    /// 資料庫初始化服務 - 建立所有資料表
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DatabaseInitializer(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// 初始化資料庫（建立所有資料表）
        /// </summary>
        public void Initialize()
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            // 建立投票活動資料表
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS VoteActivity (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    StartTime TEXT NOT NULL,
                    EndTime TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    CreatedBy TEXT NOT NULL,
                    IsDeleted INTEGER NOT NULL DEFAULT 0
                );
                CREATE INDEX IF NOT EXISTS IX_VoteActivity_StartTime_EndTime ON VoteActivity(StartTime, EndTime);
            ");

            // 建立候選人資料表
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Candidate (
                    Id TEXT PRIMARY KEY,
                    VoteActivityId TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL DEFAULT '',
                    ImagePath TEXT NOT NULL,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY (VoteActivityId) REFERENCES VoteActivity(Id)
                );
                CREATE INDEX IF NOT EXISTS IX_Candidate_VoteActivityId ON Candidate(VoteActivityId);
            ");

            // 建立可投票名單資料表
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS EligibleVoter (
                    Id TEXT PRIMARY KEY,
                    VoteActivityId TEXT NOT NULL,
                    EmployeeNo TEXT NOT NULL,
                    Name TEXT NOT NULL DEFAULT '',
                    Department TEXT NOT NULL DEFAULT '',
                    BirthDate TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY (VoteActivityId) REFERENCES VoteActivity(Id)
                );
                CREATE UNIQUE INDEX IF NOT EXISTS IX_EligibleVoter_VoteActivityId_EmployeeNo 
                    ON EligibleVoter(VoteActivityId, EmployeeNo);
                CREATE INDEX IF NOT EXISTS IX_EligibleVoter_EmployeeNo ON EligibleVoter(EmployeeNo);
            ");

            // 建立投票紀錄資料表
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS VoteRecord (
                    Id TEXT PRIMARY KEY,
                    VoteActivityId TEXT NOT NULL,
                    CandidateId TEXT NOT NULL,
                    EmployeeNo TEXT NOT NULL,
                    VotedAt TEXT NOT NULL,
                    ClientIp TEXT NOT NULL DEFAULT '',
                    UserAgent TEXT NOT NULL DEFAULT '',
                    FOREIGN KEY (VoteActivityId) REFERENCES VoteActivity(Id),
                    FOREIGN KEY (CandidateId) REFERENCES Candidate(Id)
                );
                CREATE UNIQUE INDEX IF NOT EXISTS IX_VoteRecord_VoteActivityId_EmployeeNo 
                    ON VoteRecord(VoteActivityId, EmployeeNo);
                CREATE INDEX IF NOT EXISTS IX_VoteRecord_CandidateId ON VoteRecord(CandidateId);
            ");

            // 建立管理者帳號資料表
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS AdminUser (
                    Id TEXT PRIMARY KEY,
                    Account TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL,
                    DisplayName TEXT NOT NULL,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_AdminUser_Account ON AdminUser(Account);
            ");

            // 補 Role 欄位（舊資料庫相容）
            try
            {
                connection.Execute("ALTER TABLE AdminUser ADD COLUMN Role TEXT NOT NULL DEFAULT 'admin'");
            }
            catch { /* 欄位已存在，忽略 */ }

            // 建立 Session Token 資料表
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS SessionToken (
                    Id TEXT PRIMARY KEY,
                    Token TEXT NOT NULL UNIQUE,
                    Role TEXT NOT NULL,
                    EmployeeNo TEXT,
                    AdminUserId TEXT,
                    CurrentVoteActivityId TEXT,
                    ExpireAt TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    IsRevoked INTEGER NOT NULL DEFAULT 0
                );
                CREATE INDEX IF NOT EXISTS IX_SessionToken_Token ON SessionToken(Token);
                CREATE INDEX IF NOT EXISTS IX_SessionToken_ExpireAt ON SessionToken(ExpireAt);
            ");

            // 建立驗證碼 Session 資料表
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS CaptchaSession (
                    Id TEXT PRIMARY KEY,
                    CaptchaId TEXT NOT NULL UNIQUE,
                    Code TEXT NOT NULL,
                    Purpose TEXT NOT NULL,
                    ExpireAt TEXT NOT NULL,
                    IsUsed INTEGER NOT NULL DEFAULT 0,
                    CreatedAt TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_CaptchaSession_CaptchaId ON CaptchaSession(CaptchaId);
                CREATE INDEX IF NOT EXISTS IX_CaptchaSession_ExpireAt ON CaptchaSession(ExpireAt);
            ");

            // 建立活動分區資料表
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS ActivityGroup (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL UNIQUE,
                    Description TEXT NOT NULL DEFAULT '',
                    CreatedAt TEXT NOT NULL,
                    CreatedBy TEXT NOT NULL,
                    IsDeleted INTEGER NOT NULL DEFAULT 0
                );
            ");

            // 建立管理員-分區關聯資料表
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS AdminUserGroup (
                    Id TEXT PRIMARY KEY,
                    AdminUserId TEXT NOT NULL,
                    ActivityGroupId TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY (AdminUserId) REFERENCES AdminUser(Id),
                    FOREIGN KEY (ActivityGroupId) REFERENCES ActivityGroup(Id),
                    UNIQUE(AdminUserId, ActivityGroupId)
                );
                CREATE INDEX IF NOT EXISTS IX_AdminUserGroup_AdminUserId ON AdminUserGroup(AdminUserId);
                CREATE INDEX IF NOT EXISTS IX_AdminUserGroup_ActivityGroupId ON AdminUserGroup(ActivityGroupId);
            ");

            // 補 VoteActivity.ActivityGroupId 欄位（舊資料庫相容）
            try
            {
                connection.Execute("ALTER TABLE VoteActivity ADD COLUMN ActivityGroupId TEXT");
            }
            catch { /* 欄位已存在，忽略 */ }
        }

        /// <summary>
        /// 建立預設管理者帳號（如果不存在）
        /// </summary>
        public void SeedDefaultAdmin()
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var existingAdmin = connection.QueryFirstOrDefault<int>(
                "SELECT COUNT(*) FROM AdminUser WHERE Account = @Account",
                new { Account = "admin" });

            if (existingAdmin == 0)
            {
                var now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                connection.Execute(@"
                    INSERT INTO AdminUser (Id, Account, Password, DisplayName, Role, IsEnabled, CreatedAt, UpdatedAt)
                    VALUES (@Id, @Account, @Password, @DisplayName, @Role, @IsEnabled, @CreatedAt, @UpdatedAt)",
                    new
                    {
                        Id = Guid.NewGuid().ToString(),
                        Account = "admin",
                        Password = "Admin@123",
                        DisplayName = "系統管理員",
                        Role = "super_admin",
                        IsEnabled = 1,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
            }
        }
    }
}
