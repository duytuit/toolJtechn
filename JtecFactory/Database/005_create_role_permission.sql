IF OBJECT_ID(N'[dbo].[role_permission]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[role_permission]
    (
        Id           INT IDENTITY(1,1) PRIMARY KEY,
        RoleId       INT NOT NULL,
        PermissionId INT NOT NULL,
        CreatedAt    DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UQ_RolePermission UNIQUE (RoleId, PermissionId),
        CONSTRAINT FK_RolePermission_Role FOREIGN KEY (RoleId) REFERENCES [dbo].[role](Id),
        CONSTRAINT FK_RolePermission_Permission FOREIGN KEY (PermissionId) REFERENCES [dbo].[permission](Id)
    );
END;
GO
