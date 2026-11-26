/* ============================================================================
   StockApp - tabel UserAuth (login buatan sendiri, tanpa ASP.NET Identity)
   + akun admin pertama.

   Syarat: tabel dbo.Employees sudah ada dan berisi karyawan EMP-001.
   Login admin: username  admin   password  Admin123!   (ganti setelah masuk)
   Hash dibuat dengan format PasswordHasher bawaan .NET (PBKDF2-HMAC-SHA512),
   jadi bisa diverifikasi oleh PasswordHasher<T>.VerifyHashedPassword.
   ============================================================================ */

USE StockApp;
GO

CREATE TABLE dbo.UserAuth (
    Id           INT IDENTITY(1,1) NOT NULL,
    EmployeeId   INT           NOT NULL,   -- join ke Employees.Id
    UserName     NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    IsActive     BIT           NOT NULL CONSTRAINT DF_UserAuth_IsActive    DEFAULT (1),
    FailedCount  INT           NOT NULL CONSTRAINT DF_UserAuth_FailedCount DEFAULT (0),
    LockoutEnd   DATETIME2(7)  NULL,
    LastLoginAt  DATETIME2(7)  NULL,
    CreatedAt    DATETIME2(7)  NOT NULL CONSTRAINT DF_UserAuth_CreatedAt   DEFAULT (SYSUTCDATETIME()),
    UpdatedAt    DATETIME2(7)  NULL,
    [RowVersion] ROWVERSION    NOT NULL,
    CONSTRAINT PK_UserAuth PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_UserAuth_UserName   UNIQUE (UserName),
    CONSTRAINT UQ_UserAuth_EmployeeId UNIQUE (EmployeeId),   -- 1 karyawan = 1 akun
    CONSTRAINT FK_UserAuth_Employees FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees (Id)
);
GO

-- Akun admin, tersambung ke karyawan EMP-001
INSERT INTO dbo.UserAuth (EmployeeId, UserName, PasswordHash)
SELECT e.Id, N'admin', N'AQAAAAIAAYagAAAAEFqUG2ZmpeCD/MtKcJgcIIgtwD8SHGHjkkFjyJ6E6anvgHwpCL809o8GIsJrwFWwAQ=='
FROM dbo.Employees e
WHERE e.Nik = N'EMP-001';

IF @@ROWCOUNT = 0
    PRINT N'GAGAL: karyawan EMP-001 tidak ditemukan di dbo.Employees. Akun admin belum dibuat.';
ELSE
    PRINT N'Selesai: UserAuth dibuat dan akun admin ditambahkan.';
GO

-- Cek join:
-- SELECT u.UserName, e.Nik, e.Name, e.Role
-- FROM dbo.UserAuth u JOIN dbo.Employees e ON e.Id = u.EmployeeId;
