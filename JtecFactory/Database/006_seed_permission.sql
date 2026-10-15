IF NOT EXISTS (SELECT 1 FROM [dbo].[permission])
BEGIN
    INSERT INTO [dbo].[permission] (Code, Name, Description, MenuCode, FormName, IsActive, created_at)
    VALUES
        ('DASHBOARD_VIEW', 'View Dashboard', 'Access dashboard overview', 'dashboard', 'FrmDashboard', 1, GETDATE()),

        ('PRODUCT_VIEW', 'View Product', 'Access product listing and details', 'product', 'FrmProduct', 1, GETDATE()),
        ('PRODUCT_ADD', 'Add Product', 'Create product records', 'product', 'FrmProduct', 1, GETDATE()),
        ('PRODUCT_EDIT', 'Edit Product', 'Modify product records', 'product', 'FrmProduct', 1, GETDATE()),
        ('PRODUCT_DELETE', 'Delete Product', 'Remove product records', 'product', 'FrmProduct', 1, GETDATE()),
        ('PRODUCT_EXPORT', 'Export Product', 'Export product data', 'product', 'FrmProduct', 1, GETDATE()),

        ('INVENTORY_VIEW', 'View Inventory', 'View inventory screen', 'inventory', NULL, 1, GETDATE()),
        ('INVENTORY_IMPORT', 'Import Inventory', 'Import inventory records', 'inventory', NULL, 1, GETDATE()),
        ('INVENTORY_EXPORT', 'Export Inventory', 'Export inventory data', 'inventory', NULL, 1, GETDATE()),

        ('EMPLOYEE_VIEW', 'View Employees', 'Access employee management screen', 'employee', 'FrmEmployee', 1, GETDATE()),
        ('EMPLOYEE_ADD', 'Add Employee', 'Create employee records', 'employee', 'FrmEmployee', 1, GETDATE()),
        ('EMPLOYEE_EDIT', 'Edit Employee', 'Modify employee records', 'employee', 'FrmEmployee', 1, GETDATE()),
        ('EMPLOYEE_DELETE', 'Delete Employee', 'Remove employee records', 'employee', 'FrmEmployee', 1, GETDATE()),

        ('ROLE_VIEW', 'View Roles', 'Access role management screen', 'role', 'FrmRole', 1, GETDATE()),
        ('ROLE_ADD', 'Add Role', 'Create role records', 'role', 'FrmRole', 1, GETDATE()),
        ('ROLE_EDIT', 'Edit Role', 'Modify role records', 'role', 'FrmRole', 1, GETDATE()),
        ('ROLE_DELETE', 'Delete Role', 'Remove role records', 'role', 'FrmRole', 1, GETDATE()),
        ('ROLE_PERMISSION_EDIT', 'Edit Role Permission', 'Manage role-to-permission mapping', 'role-permission', 'FrmRolePermission', 1, GETDATE());
END;
GO
