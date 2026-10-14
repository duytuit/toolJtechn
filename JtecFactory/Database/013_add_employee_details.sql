IF COL_LENGTH(N'dbo.employee', N'Department') IS NULL
    ALTER TABLE [dbo].[employee] ADD Department NVARCHAR(200) NULL;
ELSE IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.employee')
      AND name = N'Department'
      AND (TYPE_NAME(user_type_id) <> N'nvarchar' OR (max_length <> -1 AND max_length < 400))
)
    ALTER TABLE [dbo].[employee] ALTER COLUMN Department NVARCHAR(200) NULL;
GO

IF COL_LENGTH(N'dbo.employee', N'DateOfBirth') IS NULL
    ALTER TABLE [dbo].[employee] ADD DateOfBirth DATE NULL;
GO

IF COL_LENGTH(N'dbo.employee', N'Gender') IS NULL
    ALTER TABLE [dbo].[employee] ADD Gender NVARCHAR(50) NULL;
ELSE IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.employee')
      AND name = N'Gender'
      AND (TYPE_NAME(user_type_id) <> N'nvarchar' OR (max_length <> -1 AND max_length < 100))
)
    ALTER TABLE [dbo].[employee] ALTER COLUMN Gender NVARCHAR(50) NULL;
GO

IF COL_LENGTH(N'dbo.employee', N'Position') IS NULL
    ALTER TABLE [dbo].[employee] ADD Position NVARCHAR(100) NULL;
ELSE IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.employee')
      AND name = N'Position'
      AND (TYPE_NAME(user_type_id) <> N'nvarchar' OR (max_length <> -1 AND max_length < 200))
)
    ALTER TABLE [dbo].[employee] ALTER COLUMN Position NVARCHAR(100) NULL;
GO

IF COL_LENGTH(N'dbo.employee', N'JoinDate') IS NULL
    ALTER TABLE [dbo].[employee] ADD JoinDate DATE NULL;
GO

IF COL_LENGTH(N'dbo.employee', N'ProbationEndDate') IS NULL
    ALTER TABLE [dbo].[employee] ADD ProbationEndDate DATE NULL;
GO

IF COL_LENGTH(N'dbo.employee', N'LeaveDate') IS NULL
    ALTER TABLE [dbo].[employee] ADD LeaveDate DATE NULL;
GO