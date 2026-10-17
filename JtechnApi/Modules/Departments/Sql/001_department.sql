-- Chọn database của ConnectionStrings:JtecDB_Default trước khi chạy.
CREATE TABLE dbo.department
(
    id INT IDENTITY(1,1) PRIMARY KEY,
    code VARCHAR(50) NOT NULL UNIQUE,
    name NVARCHAR(150) NOT NULL,
    description NVARCHAR(1000) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NULL,
    deleted_at DATETIME2 NULL
);
