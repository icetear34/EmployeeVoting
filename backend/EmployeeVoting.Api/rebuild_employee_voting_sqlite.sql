-- 重建 EmployeeVoting SQLite DB
-- 假設：
-- 1. Guid 以 TEXT 儲存（格式：xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx）
-- 2. DateTime 以 UTC ISO-8601 TEXT 儲存（例如 2026-04-22T08:30:00.0000000Z）
-- 3. bool 以 INTEGER 儲存（0/1）

PRAGMA foreign_keys = OFF;

DROP TABLE IF EXISTS VoteRecord;
DROP TABLE IF EXISTS EligibleVoter;
DROP TABLE IF EXISTS Candidate;
DROP TABLE IF EXISTS SessionToken;
DROP TABLE IF EXISTS CaptchaSession;
DROP TABLE IF EXISTS VoteActivity;
DROP TABLE IF EXISTS AdminUser;

PRAGMA foreign_keys = ON;

CREATE TABLE AdminUser (
    Id              TEXT    NOT NULL PRIMARY KEY,
    Account         TEXT    NOT NULL,
    Password        TEXT    NOT NULL,
    DisplayName     TEXT    NOT NULL,
    IsEnabled       INTEGER NOT NULL DEFAULT 1 CHECK (IsEnabled IN (0, 1)),
    CreatedAt       TEXT    NOT NULL,
    UpdatedAt       TEXT    NOT NULL
);

CREATE UNIQUE INDEX IX_AdminUser_Account ON AdminUser(Account);

CREATE TABLE VoteActivity (
    Id                      TEXT    NOT NULL PRIMARY KEY,
    ActivityCode            TEXT    NOT NULL,
    Name                    TEXT    NOT NULL,
    Description             TEXT    NULL,
    StartTime               TEXT    NOT NULL,
    EndTime                 TEXT    NOT NULL,
    CreatedAt               TEXT    NOT NULL,
    CreatedBy               TEXT    NOT NULL,
    IsDeleted               INTEGER NOT NULL DEFAULT 0 CHECK (IsDeleted IN (0, 1)),
    IsResultViewable        INTEGER NOT NULL DEFAULT 0 CHECK (IsResultViewable IN (0, 1)),
    ResultViewStartTime     TEXT    NULL,
    ResultViewEndTime       TEXT    NULL
);

CREATE UNIQUE INDEX IX_VoteActivity_ActivityCode ON VoteActivity(ActivityCode);
CREATE INDEX IX_VoteActivity_IsDeleted_CreatedAt ON VoteActivity(IsDeleted, CreatedAt DESC);
CREATE INDEX IX_VoteActivity_StartTime_EndTime ON VoteActivity(StartTime, EndTime);

CREATE TABLE Candidate (
    Id              TEXT    NOT NULL PRIMARY KEY,
    VoteActivityId  TEXT    NOT NULL,
    Name            TEXT    NOT NULL,
    Description     TEXT    NOT NULL DEFAULT '',
    ImagePath       TEXT    NOT NULL DEFAULT '',
    SortOrder       INTEGER NOT NULL DEFAULT 1,
    IsEnabled       INTEGER NOT NULL DEFAULT 1 CHECK (IsEnabled IN (0, 1)),
    CreatedAt       TEXT    NOT NULL,
    CONSTRAINT FK_Candidate_VoteActivity
        FOREIGN KEY (VoteActivityId) REFERENCES VoteActivity(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Candidate_VoteActivityId_SortOrder ON Candidate(VoteActivityId, SortOrder);
CREATE INDEX IX_Candidate_VoteActivityId_IsEnabled ON Candidate(VoteActivityId, IsEnabled);

CREATE TABLE EligibleVoter (
    Id              TEXT    NOT NULL PRIMARY KEY,
    VoteActivityId  TEXT    NOT NULL,
    EmployeeNo      TEXT    NOT NULL,
    Name            TEXT    NOT NULL,
    Department      TEXT    NOT NULL DEFAULT '',
    BirthDate       TEXT    NOT NULL,
    CreatedAt       TEXT    NOT NULL,
    CONSTRAINT FK_EligibleVoter_VoteActivity
        FOREIGN KEY (VoteActivityId) REFERENCES VoteActivity(Id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IX_EligibleVoter_Activity_EmployeeNo
    ON EligibleVoter(VoteActivityId, EmployeeNo);
CREATE INDEX IX_EligibleVoter_VoteActivityId ON EligibleVoter(VoteActivityId);

CREATE TABLE VoteRecord (
    Id              TEXT    NOT NULL PRIMARY KEY,
    VoteActivityId  TEXT    NOT NULL,
    CandidateId     TEXT    NOT NULL,
    EmployeeNo      TEXT    NOT NULL,
    VotedAt         TEXT    NOT NULL,
    ClientIp        TEXT    NOT NULL DEFAULT '',
    UserAgent       TEXT    NOT NULL DEFAULT '',
    CONSTRAINT FK_VoteRecord_VoteActivity
        FOREIGN KEY (VoteActivityId) REFERENCES VoteActivity(Id) ON DELETE CASCADE,
    CONSTRAINT FK_VoteRecord_Candidate
        FOREIGN KEY (CandidateId) REFERENCES Candidate(Id) ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IX_VoteRecord_Activity_EmployeeNo
    ON VoteRecord(VoteActivityId, EmployeeNo);
CREATE INDEX IX_VoteRecord_CandidateId ON VoteRecord(CandidateId);
CREATE INDEX IX_VoteRecord_VoteActivityId_VotedAt ON VoteRecord(VoteActivityId, VotedAt DESC);

CREATE TABLE SessionToken (
    Id                    TEXT    NOT NULL PRIMARY KEY,
    Token                 TEXT    NOT NULL,
    Role                  TEXT    NOT NULL,
    EmployeeNo            TEXT    NULL,
    AdminUserId           TEXT    NULL,
    CurrentVoteActivityId TEXT    NULL,
    ExpireAt              TEXT    NOT NULL,
    CreatedAt             TEXT    NOT NULL,
    IsRevoked             INTEGER NOT NULL DEFAULT 0 CHECK (IsRevoked IN (0, 1)),
    CONSTRAINT FK_SessionToken_AdminUser
        FOREIGN KEY (AdminUserId) REFERENCES AdminUser(Id) ON DELETE SET NULL,
    CONSTRAINT FK_SessionToken_VoteActivity
        FOREIGN KEY (CurrentVoteActivityId) REFERENCES VoteActivity(Id) ON DELETE SET NULL
);

CREATE UNIQUE INDEX IX_SessionToken_Token ON SessionToken(Token);
CREATE INDEX IX_SessionToken_AdminUserId ON SessionToken(AdminUserId);
CREATE INDEX IX_SessionToken_EmployeeNo ON SessionToken(EmployeeNo);
CREATE INDEX IX_SessionToken_ExpireAt_IsRevoked ON SessionToken(ExpireAt, IsRevoked);

CREATE TABLE CaptchaSession (
    Id              TEXT    NOT NULL PRIMARY KEY,
    CaptchaId       TEXT    NOT NULL,
    Code            TEXT    NOT NULL,
    Purpose         TEXT    NOT NULL,
    ExpireAt        TEXT    NOT NULL,
    IsUsed          INTEGER NOT NULL DEFAULT 0 CHECK (IsUsed IN (0, 1)),
    CreatedAt       TEXT    NOT NULL
);

CREATE UNIQUE INDEX IX_CaptchaSession_CaptchaId ON CaptchaSession(CaptchaId);
CREATE INDEX IX_CaptchaSession_Purpose_ExpireAt ON CaptchaSession(Purpose, ExpireAt);
CREATE INDEX IX_CaptchaSession_ExpireAt_IsUsed ON CaptchaSession(ExpireAt, IsUsed);

INSERT OR IGNORE INTO AdminUser (
    Id,
    Account,
    Password,
    DisplayName,
    IsEnabled,
    CreatedAt,
    UpdatedAt
) VALUES (
    '11111111-1111-1111-1111-111111111111',
    'sa',
    'sa',
    '系統管理員',
    1,
    strftime('%Y-%m-%dT%H:%M:%SZ','now'),
    strftime('%Y-%m-%dT%H:%M:%SZ','now')
);

-- 建議在應用程式啟動時執行：
-- PRAGMA journal_mode = WAL;
-- PRAGMA synchronous = NORMAL;
-- PRAGMA busy_timeout = 5000;
