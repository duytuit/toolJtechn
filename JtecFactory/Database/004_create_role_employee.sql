IF OBJECT_ID(N'[dbo].[role_employee]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[role_employee]
    (
        Id         INT IDENTITY(1,1) PRIMARY KEY,
        RoleId     INT NOT NULL,
        EmployeeId INT NOT NULL,
        CreatedAt  DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UQ_RoleEmployee UNIQUE (RoleId, EmployeeId),
        CONSTRAINT FK_RoleEmployee_Role FOREIGN KEY (RoleId) REFERENCES [dbo].[role](Id),
        CONSTRAINT FK_RoleEmployee_Employee FOREIGN KEY (EmployeeId) REFERENCES [dbo].[employee](Id)
    );
END;
GO
