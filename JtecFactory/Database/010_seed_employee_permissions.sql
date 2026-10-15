DECLARE @Permissions TABLE
(
    Code VARCHAR(100) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    MenuCode VARCHAR(100) NOT NULL,
    FormName VARCHAR(200) NOT NULL
);

INSERT INTO @Permissions (Code, Name, Description, MenuCode, FormName)
VALUES
    ('EMPLOYEE_ROLES_VIEW', N'Xem nhóm quyền nhân viên', N'Mở màn hình gán nhóm quyền cho nhân viên', 'employee_roles', 'FrmEmployeeRoles'),
    ('EMPLOYEE_ROLES_ADD', N'Thêm gán nhóm quyền', N'Gán nhóm quyền cho nhân viên', 'employee_roles', 'FrmEmployeeRoles'),
    ('EMPLOYEE_ROLES_EDIT', N'Sửa gán nhóm quyền', N'Cập nhật nhóm quyền của nhân viên', 'employee_roles', 'FrmEmployeeRoles'),
    ('EMPLOYEE_ROLES_DELETE', N'Xóa gán nhóm quyền', N'Gỡ nhóm quyền khỏi nhân viên', 'employee_roles', 'FrmEmployeeRoles'),
    ('ROLE_VIEW', N'Xem nhóm quyền', N'Mở màn hình quản lý nhóm quyền', 'role', 'FrmRole'),
    ('ROLE_ADD', N'Tạo nhóm quyền', N'Tạo nhóm quyền mới', 'role', 'FrmRole'),
    ('ROLE_EDIT', N'Sửa nhóm quyền', N'Cập nhật thông tin nhóm quyền', 'role', 'FrmRole'),
    ('ROLE_DELETE', N'Xóa nhóm quyền', N'Xóa mềm nhóm quyền', 'role', 'FrmRole'),
    ('ROLE_PERMISSION_VIEW', N'Xem phân quyền nhóm', N'Mở màn hình phân quyền nhóm', 'role_permission', 'FrmRolePermission'),
    ('ROLE_PERMISSION_ADD', N'Thêm phân quyền nhóm', N'Cấp quyền thao tác cho nhóm', 'role_permission', 'FrmRolePermission'),
    ('ROLE_PERMISSION_EDIT', N'Sửa phân quyền nhóm', N'Cập nhật quyền thao tác của nhóm', 'role_permission', 'FrmRolePermission'),
    ('ROLE_PERMISSION_DELETE', N'Xóa phân quyền nhóm', N'Thu hồi quyền thao tác của nhóm', 'role_permission', 'FrmRolePermission'),
    ('ROLE_PERMISSION_EXPORT', N'Xuất phân quyền nhóm', N'Xuất cấu hình phân quyền', 'role_permission', 'FrmRolePermission');

INSERT INTO [dbo].[permission] (Code, Name, Description, MenuCode, FormName, IsActive, created_at)
SELECT source.Code, source.Name, source.Description, source.MenuCode, source.FormName, 1, SYSUTCDATETIME()
FROM @Permissions AS source
WHERE NOT EXISTS
(
    SELECT 1
    FROM [dbo].[permission] AS target
    WHERE target.Code = source.Code
);

UPDATE target
SET IsActive = 1,
    deleted_at = NULL,
    updated_at = SYSUTCDATETIME()
FROM [dbo].[permission] AS target
INNER JOIN @Permissions AS source ON source.Code = target.Code;
GO

DECLARE @AdminRoleId INT = (SELECT Id FROM [dbo].[role] WHERE Code = 'ADMIN' AND deleted_at IS NULL);

IF @AdminRoleId IS NOT NULL
BEGIN
    INSERT INTO [dbo].[role_permission] (RoleId, PermissionId, created_at)
    SELECT @AdminRoleId, permission.Id, SYSUTCDATETIME()
    FROM [dbo].[permission] AS permission
    WHERE permission.Code IN
    (
        'EMPLOYEE_ROLES_VIEW', 'EMPLOYEE_ROLES_ADD', 'EMPLOYEE_ROLES_EDIT', 'EMPLOYEE_ROLES_DELETE',
        'ROLE_VIEW', 'ROLE_ADD', 'ROLE_EDIT', 'ROLE_DELETE',
        'ROLE_PERMISSION_VIEW', 'ROLE_PERMISSION_ADD', 'ROLE_PERMISSION_EDIT',
        'ROLE_PERMISSION_DELETE', 'ROLE_PERMISSION_EXPORT'
    )
      AND permission.deleted_at IS NULL
      AND NOT EXISTS
      (
          SELECT 1
          FROM [dbo].[role_permission] AS assignment
          WHERE assignment.RoleId = @AdminRoleId
            AND assignment.PermissionId = permission.Id
      );
END;
GO
