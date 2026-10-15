IF OBJECT_ID(N'[dbo].[permission]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[permission]
    (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Code        VARCHAR(100) NOT NULL UNIQUE,
        Name        NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        MenuCode    VARCHAR(100) NULL,
        FormName    VARCHAR(200) NULL,
        IsActive    BIT NOT NULL DEFAULT 1,
        created_at   DATETIME2 NOT NULL DEFAULT GETDATE(),
        updated_at   DATETIME2 NULL,
        deleted_at   DATETIME2 NULL
    );
END;
GO

IF COL_LENGTH(N'dbo.permission', N'deleted_at') IS NULL
    ALTER TABLE [dbo].[permission] ADD deleted_at DATETIME2 NULL;
GO
