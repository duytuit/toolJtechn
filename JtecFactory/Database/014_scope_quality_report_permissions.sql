DECLARE @Menus TABLE
(
    ParentCode VARCHAR(100) NOT NULL,
    ParentName NVARCHAR(200) NOT NULL,
    ChildCode VARCHAR(100) NOT NULL,
    ChildName NVARCHAR(200) NOT NULL
);

INSERT INTO @Menus (ParentCode, ParentName, ChildCode, ChildName)
VALUES
    ('buredo', N'Buredo', 'quality-report', N'Quality Report'),
    ('laprap', N'Lắp Ráp', 'quality-report', N'Quality Report'),
    ('thongmach', N'Thông Mạch', 'quality-report', N'Quality Report'),
    ('kiemtra', N'Kiểm Tra', 'quality-report', N'Quality Report'),
    ('kho', N'Kho', 'quality-report', N'Quality Report'),
    ('ga', N'GA', 'quality-report', N'Quality Report'),
    ('qlsx', N'QLSX', 'quality-report', N'Quality Report'),
    ('congcu', N'Công Cụ', 'quality-report', N'Quality Report');

DECLARE @Actions TABLE
(
    ActionCode VARCHAR(20) NOT NULL,
    ActionName NVARCHAR(50) NOT NULL
);

INSERT INTO @Actions (ActionCode, ActionName)
VALUES
    ('VIEW', N'Xem'),
    ('ADD', N'Thêm'),
    ('EDIT', N'Sửa'),
    ('DELETE', N'Xóa'),
    ('EXPORT', N'Xuất');

INSERT INTO dbo.permission (Code, Name, Description, MenuCode, FormName, IsActive, created_at, updated_at)
SELECT UPPER(menu.ParentCode + '_' + REPLACE(menu.ChildCode, '-', '_') + '_' + action.ActionCode),
       menu.ChildName + N' (' + menu.ParentName + N') - ' + action.ActionName,
       action.ActionName + N' quyền cho ' + menu.ParentName + N' / ' + menu.ChildName,
       menu.ParentCode + '.' + menu.ChildCode,
       NULL,
       1,
       SYSUTCDATETIME(),
       SYSUTCDATETIME()
FROM @Menus AS menu
CROSS JOIN @Actions AS action
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.permission AS existing
    WHERE existing.Code = UPPER(menu.ParentCode + '_' + REPLACE(menu.ChildCode, '-', '_') + '_' + action.ActionCode)
);

UPDATE permission
SET IsActive = 1,
    deleted_at = NULL,
    MenuCode = menu.ParentCode + '.' + menu.ChildCode,
    updated_at = SYSUTCDATETIME()
FROM dbo.permission AS permission
INNER JOIN @Menus AS menu
    ON permission.Code LIKE UPPER(menu.ParentCode + '_' + REPLACE(menu.ChildCode, '-', '_') + '_%');

INSERT INTO dbo.role_permission (RoleId, PermissionId, created_at)
SELECT DISTINCT oldAssignment.RoleId, scopedPermission.Id, SYSUTCDATETIME()
FROM @Actions AS action
INNER JOIN dbo.permission AS oldPermission
    ON oldPermission.Code = 'QUALITY_REPORT_' + action.ActionCode
INNER JOIN dbo.role_permission AS oldAssignment
    ON oldAssignment.PermissionId = oldPermission.Id
CROSS JOIN @Menus AS menu
INNER JOIN dbo.permission AS scopedPermission
    ON scopedPermission.Code = UPPER(menu.ParentCode + '_' + REPLACE(menu.ChildCode, '-', '_') + '_' + action.ActionCode)
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.role_permission AS existing
    WHERE existing.RoleId = oldAssignment.RoleId
      AND existing.PermissionId = scopedPermission.Id
);

DELETE assignment
FROM dbo.role_permission AS assignment
INNER JOIN dbo.permission AS permission ON permission.Id = assignment.PermissionId
WHERE permission.Code LIKE 'QUALITY_REPORT[_]%';

UPDATE dbo.permission
SET IsActive = 0,
    deleted_at = COALESCE(deleted_at, SYSUTCDATETIME()),
    updated_at = SYSUTCDATETIME()
WHERE Code LIKE 'QUALITY_REPORT[_]%';
GO