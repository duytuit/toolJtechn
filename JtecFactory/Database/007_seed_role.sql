IF NOT EXISTS (SELECT 1 FROM [dbo].[role])
BEGIN
    INSERT INTO [dbo].[role] (Name, Code, Description, IsActive, CreatedAt)
    VALUES
        (N'Administrator', 'ADMIN', N'Full system administrator', 1, GETDATE()),
        (N'Manager', 'MANAGER', N'Operational manager', 1, GETDATE()),
        (N'Warehouse User', 'WAREHOUSE', N'Warehouse and inventory operations', 1, GETDATE()),
        (N'Accounting User', 'ACCOUNTING', N'Finance and accounting user', 1, GETDATE());
END;
GO
