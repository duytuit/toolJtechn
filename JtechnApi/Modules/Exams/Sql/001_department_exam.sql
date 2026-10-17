-- Chọn database SQL Server của ConnectionStrings:JtecDB_Default trước khi chạy.
CREATE TABLE dbo.department_exam
(
    id INT IDENTITY(1,1) PRIMARY KEY,
    department_id INT NOT NULL,
    code VARCHAR(50) NOT NULL UNIQUE,
    name NVARCHAR(200) NOT NULL,
    description NVARCHAR(1000) NULL,
    minutes INT NOT NULL DEFAULT 30,
    definition_json NVARCHAR(MAX) NOT NULL,
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NULL,
    deleted_at DATETIME2 NULL
);
CREATE INDEX IX_department_exam_department_id ON dbo.department_exam(department_id);
