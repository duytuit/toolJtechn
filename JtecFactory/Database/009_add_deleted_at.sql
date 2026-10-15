IF COL_LENGTH(N'dbo.role', N'deleted_at') IS NULL
    ALTER TABLE [dbo].[role] ADD deleted_at DATETIME2 NULL;
GO

IF COL_LENGTH(N'dbo.employee', N'deleted_at') IS NULL
    ALTER TABLE [dbo].[employee] ADD deleted_at DATETIME2 NULL;
GO

IF COL_LENGTH(N'dbo.permission', N'deleted_at') IS NULL
    ALTER TABLE [dbo].[permission] ADD deleted_at DATETIME2 NULL;
GO
