IF COL_LENGTH(N'dbo.role', N'DeletedAt') IS NULL
    ALTER TABLE [dbo].[role] ADD DeletedAt DATETIME2 NULL;
GO

IF COL_LENGTH(N'dbo.employee', N'DeletedAt') IS NULL
    ALTER TABLE [dbo].[employee] ADD DeletedAt DATETIME2 NULL;
GO

IF COL_LENGTH(N'dbo.permission', N'DeletedAt') IS NULL
    ALTER TABLE [dbo].[permission] ADD DeletedAt DATETIME2 NULL;
GO
