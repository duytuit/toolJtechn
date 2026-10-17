-- Chạy trên database SQL Server của ConnectionStrings:JtecDB_Default.
-- Cần có dbo.employee và dbo.department trước khi chạy.
CREATE TABLE dbo.employee_department
(
    id INT IDENTITY(1,1) PRIMARY KEY,
    employee_id INT NOT NULL,
    department_id INT NOT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NULL,
    deleted_at DATETIME2 NULL,
    CONSTRAINT UQ_employee_department UNIQUE (employee_id, department_id)
);

CREATE INDEX IX_employee_department_department_id ON dbo.employee_department(department_id);
