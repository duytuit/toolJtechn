IF NOT EXISTS (SELECT 1 FROM [dbo].[employee] WHERE Username = 'admin')
BEGIN
    INSERT INTO [dbo].[employee] (EmployeeCode, Username, FullName, PasswordHash, Email, Phone, IsActive, CreatedAt)
    VALUES
        ('ADM001', 'admin', N'Development Administrator', '100000$ZF8mVuGXC8ejyZ+HnMzTKQ==$Nui5aqkduYOts8ugncS/kcYTmGpwAAO527k+asXaoRY=', 'admin@local.test', '0900000001', 1, GETDATE());
END;
GO

DECLARE @AdminEmployeeId INT = (SELECT Id FROM [dbo].[employee] WHERE Username = 'admin');
DECLARE @AdminRoleId INT = (SELECT Id FROM [dbo].[role] WHERE Code = 'ADMIN');

IF @AdminEmployeeId IS NOT NULL AND @AdminRoleId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[role_employee] WHERE RoleId = @AdminRoleId AND EmployeeId = @AdminEmployeeId)
BEGIN
    INSERT INTO [dbo].[role_employee] (RoleId, EmployeeId, CreatedAt)
    VALUES (@AdminRoleId, @AdminEmployeeId, GETDATE());
END;
GO

DECLARE @AdminRoleId INT = (SELECT Id FROM [dbo].[role] WHERE Code = 'ADMIN');

IF @AdminRoleId IS NOT NULL
BEGIN
    DELETE rp
    FROM [dbo].[role_permission] rp
    WHERE rp.RoleId = @AdminRoleId;

    INSERT INTO [dbo].[role_permission] (RoleId, PermissionId, CreatedAt)
    SELECT @AdminRoleId, p.Id, GETDATE()
    FROM [dbo].[permission] p
    WHERE p.IsActive = 1;
END;
GO
