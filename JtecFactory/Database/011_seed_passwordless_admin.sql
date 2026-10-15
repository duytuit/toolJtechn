SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM [dbo].[role] WHERE Code = 'ADMIN')
BEGIN
    INSERT INTO [dbo].[role] (Name, Code, Description, IsActive, created_at)
    VALUES (N'Administrator', 'ADMIN', N'Full system administrator', 1, SYSUTCDATETIME());
END;

UPDATE [dbo].[role]
SET IsActive = 1, deleted_at = NULL
WHERE Code = 'ADMIN';

IF NOT EXISTS (SELECT 1 FROM [dbo].[employee] WHERE Username = 'admin')
BEGIN
    INSERT INTO [dbo].[employee]
        (EmployeeCode, Username, FullName, PasswordHash, Email, Phone, IsActive, created_at)
    VALUES
        ('ADM001', 'admin', N'System Administrator',
         'PASSWORDLESS_LOGIN_DISABLED_PASSWORD_AUTH',
         'admin@local.test', NULL, 1, SYSUTCDATETIME());
END;

UPDATE [dbo].[employee]
SET IsActive = 1, deleted_at = NULL
WHERE Username = 'admin';

DECLARE @AdminEmployeeId INT =
    (SELECT Id FROM [dbo].[employee] WHERE Username = 'admin');
DECLARE @AdminRoleId INT =
    (SELECT Id FROM [dbo].[role] WHERE Code = 'ADMIN');

IF NOT EXISTS
(
    SELECT 1 FROM [dbo].[role_employee]
    WHERE EmployeeId = @AdminEmployeeId AND RoleId = @AdminRoleId
)
BEGIN
    INSERT INTO [dbo].[role_employee] (RoleId, EmployeeId, created_at)
    VALUES (@AdminRoleId, @AdminEmployeeId, SYSUTCDATETIME());
END;

INSERT INTO [dbo].[role_permission] (RoleId, PermissionId, created_at)
SELECT @AdminRoleId, permission.Id, SYSUTCDATETIME()
FROM [dbo].[permission] AS permission
WHERE permission.IsActive = 1
  AND permission.deleted_at IS NULL
  AND NOT EXISTS
  (
      SELECT 1
      FROM [dbo].[role_permission] AS assignment
      WHERE assignment.RoleId = @AdminRoleId
        AND assignment.PermissionId = permission.Id
  );

COMMIT TRANSACTION;
