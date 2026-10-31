-- Chạy một lần nếu dbo.online_exam_attempt đã được tạo từ phiên bản schema trước của 002_online_exam_attempt.sql.
-- Bổ sung các cột khóa và snapshot để luồng thi lưu/đọc trực tiếp từ SQL Server.
IF COL_LENGTH('dbo.online_exam_attempt', 'attempt_key') IS NULL
    ALTER TABLE dbo.online_exam_attempt ADD attempt_key UNIQUEIDENTIFIER NULL;

IF COL_LENGTH('dbo.online_exam_attempt', 'employee_id') IS NULL
    ALTER TABLE dbo.online_exam_attempt ADD employee_id INT NULL;

IF COL_LENGTH('dbo.online_exam_attempt', 'catalog_exam_id') IS NULL
    ALTER TABLE dbo.online_exam_attempt ADD catalog_exam_id INT NULL;

IF COL_LENGTH('dbo.online_exam_attempt', 'started_at') IS NULL
    ALTER TABLE dbo.online_exam_attempt ADD started_at DATETIME2 NULL;

IF COL_LENGTH('dbo.online_exam_attempt', 'expires_at') IS NULL
    ALTER TABLE dbo.online_exam_attempt ADD expires_at DATETIME2 NULL;

IF COL_LENGTH('dbo.online_exam_attempt', 'submitted_at') IS NULL
    ALTER TABLE dbo.online_exam_attempt ADD submitted_at DATETIME2 NULL;

IF COL_LENGTH('dbo.online_exam_attempt', 'attempt_json') IS NULL
    ALTER TABLE dbo.online_exam_attempt ADD attempt_json NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.online_exam_attempt', 'code') IS NOT NULL
    ALTER TABLE dbo.online_exam_attempt ALTER COLUMN code NVARCHAR(50) NULL;

IF COL_LENGTH('dbo.online_exam_attempt', 'scores') IS NOT NULL
    ALTER TABLE dbo.online_exam_attempt ALTER COLUMN scores DECIMAL(10,4) NULL;

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_online_exam_attempt_attempt_key'
      AND object_id = OBJECT_ID('dbo.online_exam_attempt')
)
    EXEC(N'CREATE UNIQUE INDEX IX_online_exam_attempt_attempt_key
        ON dbo.online_exam_attempt(attempt_key)
        WHERE attempt_key IS NOT NULL');

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_online_exam_attempt_employee_exam_submission'
      AND object_id = OBJECT_ID('dbo.online_exam_attempt')
)
    EXEC(N'CREATE INDEX IX_online_exam_attempt_employee_exam_submission
        ON dbo.online_exam_attempt(employee_id, catalog_exam_id, submitted_at)');
