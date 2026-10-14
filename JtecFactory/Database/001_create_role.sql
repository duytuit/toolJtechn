IF OBJECT_ID(N'[dbo].[role]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[role]
    (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Name        NVARCHAR(100) NOT NULL,
        Code        VARCHAR(50) NOT NULL UNIQUE,
        Description NVARCHAR(500) NULL,
        IsActive    BIT NOT NULL DEFAULT 1,
        CreatedAt   DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt   DATETIME2 NULL,
        DeletedAt   DATETIME2 NULL
    );
END;
GO

IF COL_LENGTH(N'dbo.role', N'DeletedAt') IS NULL
    ALTER TABLE [dbo].[role] ADD DeletedAt DATETIME2 NULL;
GO
