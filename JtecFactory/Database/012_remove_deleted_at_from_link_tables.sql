SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.role_employee', N'deleted_at') IS NOT NULL
BEGIN
    EXEC(N'
        DELETE FROM [dbo].[role_employee] WHERE deleted_at IS NOT NULL;

        ;WITH RankedAssignments AS
        (
            SELECT Id,
                   ROW_NUMBER() OVER
                   (
                       PARTITION BY RoleId, EmployeeId
                       ORDER BY created_at DESC,
                                Id DESC
                   ) AS RowNumber
            FROM [dbo].[role_employee]
        )
        DELETE FROM RankedAssignments WHERE RowNumber > 1;
    ');
END;

IF EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.role_employee')
      AND name = N'UX_RoleEmployee_Active'
)
    DROP INDEX UX_RoleEmployee_Active ON [dbo].[role_employee];

IF COL_LENGTH(N'dbo.role_employee', N'deleted_at') IS NOT NULL
    ALTER TABLE [dbo].[role_employee] DROP COLUMN deleted_at;

IF NOT EXISTS
(
    SELECT 1 FROM sys.key_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.role_employee')
      AND name = N'UQ_RoleEmployee'
)
    ALTER TABLE [dbo].[role_employee]
        ADD CONSTRAINT UQ_RoleEmployee UNIQUE (RoleId, EmployeeId);

IF COL_LENGTH(N'dbo.role_permission', N'deleted_at') IS NOT NULL
BEGIN
    EXEC(N'
        DELETE FROM [dbo].[role_permission] WHERE deleted_at IS NOT NULL;

        ;WITH RankedAssignments AS
        (
            SELECT Id,
                   ROW_NUMBER() OVER
                   (
                       PARTITION BY RoleId, PermissionId
                       ORDER BY created_at DESC,
                                Id DESC
                   ) AS RowNumber
            FROM [dbo].[role_permission]
        )
        DELETE FROM RankedAssignments WHERE RowNumber > 1;
    ');
END;

IF EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.role_permission')
      AND name = N'UX_RolePermission_Active'
)
    DROP INDEX UX_RolePermission_Active ON [dbo].[role_permission];

IF COL_LENGTH(N'dbo.role_permission', N'deleted_at') IS NOT NULL
    ALTER TABLE [dbo].[role_permission] DROP COLUMN deleted_at;

IF NOT EXISTS
(
    SELECT 1 FROM sys.key_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.role_permission')
      AND name = N'UQ_RolePermission'
)
    ALTER TABLE [dbo].[role_permission]
        ADD CONSTRAINT UQ_RolePermission UNIQUE (RoleId, PermissionId);

COMMIT TRANSACTION;
