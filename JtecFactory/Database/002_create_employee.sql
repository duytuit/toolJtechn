IF OBJECT_ID(N'[dbo].[employee]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[employee]
    (
        Id            INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeCode  VARCHAR(50) NOT NULL UNIQUE,
        Username      VARCHAR(100) NOT NULL UNIQUE,
        FullName      NVARCHAR(200) NOT NULL,
        PasswordHash  VARCHAR(500) NOT NULL,
        Email         VARCHAR(200) NULL,
        Phone         VARCHAR(50) NULL,
        Department    NVARCHAR(200) NULL,
        DateOfBirth   DATE NULL,
        Gender        NVARCHAR(50) NULL,
        Position      NVARCHAR(100) NULL,
        JoinDate      DATE NULL,
        ProbationEndDate DATE NULL,
        LeaveDate     DATE NULL,
        IsActive      BIT NOT NULL DEFAULT 1,
        CreatedAt     DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt     DATETIME2 NULL,
        DeletedAt     DATETIME2 NULL
    );
END;
GO

IF COL_LENGTH(N'dbo.employee', N'DeletedAt') IS NULL
    ALTER TABLE [dbo].[employee] ADD DeletedAt DATETIME2 NULL;
GO
