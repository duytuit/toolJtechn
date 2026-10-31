-- Chọn database SQL Server của ConnectionStrings:JtecDB_Default trước khi chạy.
CREATE TABLE dbo.online_exam_attempt
(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    attempt_key UNIQUEIDENTIFIER NOT NULL,
    employee_id INT NOT NULL,
    catalog_exam_id INT NULL,
    name NVARCHAR(200) NOT NULL,
    code NVARCHAR(50) NULL,
    dept_id INT NOT NULL,
    cycle_name NVARCHAR(200) NULL,
    create_date DATE NULL,
    results INT NULL,
    total_questions INT NULL,
    status BIT NULL,
    confirm BIT NOT NULL DEFAULT 0,
    counting_time INT NULL,
    limit_time INT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NULL,
    deleted_at DATETIME2 NULL,
    updated_by NVARCHAR(50) NULL,
    deleted_by NVARCHAR(50) NULL,
    mission INT NOT NULL DEFAULT 1,
    scores DECIMAL(10,4) NULL,
    examinations NVARCHAR(200) NULL,
    date_examinations NVARCHAR(200) NULL,
    [type] NVARCHAR(100) NULL,
    fail_aws NVARCHAR(MAX) NULL,
    newbie BIT NOT NULL DEFAULT 0,
    started_at DATETIME2 NOT NULL,
    expires_at DATETIME2 NOT NULL,
    submitted_at DATETIME2 NULL,
    attempt_json NVARCHAR(MAX) NOT NULL,
    CONSTRAINT CK_online_exam_attempt_results
        CHECK (results IS NULL OR results >= 0),
    CONSTRAINT CK_online_exam_attempt_total_questions
        CHECK (total_questions IS NULL OR total_questions >= 0),
    CONSTRAINT CK_online_exam_attempt_counting_time
        CHECK (counting_time IS NULL OR counting_time >= 0),
    CONSTRAINT CK_online_exam_attempt_limit_time
        CHECK (limit_time IS NULL OR limit_time > 0),
    CONSTRAINT CK_online_exam_attempt_mission
        CHECK (mission > 0),
    CONSTRAINT CK_online_exam_attempt_fail_aws_json
        CHECK (fail_aws IS NULL OR ISJSON(fail_aws) = 1),
    CONSTRAINT CK_online_exam_attempt_attempt_json
        CHECK (ISJSON(attempt_json) = 1)
);

CREATE INDEX IX_online_exam_attempt_code_create_date
    ON dbo.online_exam_attempt(code, create_date);

CREATE INDEX IX_online_exam_attempt_dept_id_create_date
    ON dbo.online_exam_attempt(dept_id, create_date);

CREATE UNIQUE INDEX IX_online_exam_attempt_attempt_key
    ON dbo.online_exam_attempt(attempt_key);

CREATE INDEX IX_online_exam_attempt_employee_exam_submission
    ON dbo.online_exam_attempt(employee_id, catalog_exam_id, submitted_at);
