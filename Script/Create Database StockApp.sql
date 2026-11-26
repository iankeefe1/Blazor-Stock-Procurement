/* ============================================================================
   StockApp - skrip pembuatan database dan tabel aplikasi
   Sesuai dokumen "Rancangan Database StockApp".

   - Jalankan di SSMS / Azure Data Studio / sqlcmd (butuh SQL Server 2016+).
   - Untuk database baru. Kalau tabelnya sudah ada, skrip berhenti dengan error
     (aman, tidak menimpa data). Untuk mengulang dari nol saat development,
     jalankan dulu StockApp_00_reset_dev.sql.
   - Semua tabel punya PRIMARY KEY.
   - Tabel ASP.NET Identity (AspNetUsers, AspNetRoles, dst.) tidak ada di sini:
     dibuat oleh EF Core (migrasi Identity). Tabel Employees menyambung ke
     AspNetUsers lewat kolom UserId (FK dipasang otomatis kalau tabelnya ada).
   ============================================================================ */

USE master;
GO

IF DB_ID(N'StockApp') IS NULL
    CREATE DATABASE StockApp;
GO

USE StockApp;
GO

SET NOCOUNT ON;
GO

/* ============================================================================
   1. DATA MASTER
   ============================================================================ */

CREATE TABLE dbo.Warehouses (
    Id          INT IDENTITY(1,1) NOT NULL,
    Code        NVARCHAR(20)  NOT NULL,
    Name        NVARCHAR(100) NOT NULL,
    Address     NVARCHAR(300) NULL,
    IsActive    BIT           NOT NULL CONSTRAINT DF_Warehouses_IsActive  DEFAULT (1),
    CreatedAt   DATETIME2(7)  NOT NULL CONSTRAINT DF_Warehouses_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt   DATETIME2(7)  NULL,
    [RowVersion] ROWVERSION   NOT NULL,
    CONSTRAINT PK_Warehouses PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Warehouses_Code UNIQUE (Code)
);
GO

CREATE TABLE dbo.Categories (
    Id          INT IDENTITY(1,1) NOT NULL,
    Name        NVARCHAR(100) NOT NULL,
    Description NVARCHAR(300) NULL,
    IsActive    BIT           NOT NULL CONSTRAINT DF_Categories_IsActive  DEFAULT (1),
    CreatedAt   DATETIME2(7)  NOT NULL CONSTRAINT DF_Categories_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt   DATETIME2(7)  NULL,
    [RowVersion] ROWVERSION   NOT NULL,
    CONSTRAINT PK_Categories PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Categories_Name UNIQUE (Name)
);
GO

CREATE TABLE dbo.Units (
    Id          INT IDENTITY(1,1) NOT NULL,
    Code        NVARCHAR(20)  NOT NULL,
    Name        NVARCHAR(50)  NOT NULL,
    IsActive    BIT           NOT NULL CONSTRAINT DF_Units_IsActive  DEFAULT (1),
    CreatedAt   DATETIME2(7)  NOT NULL CONSTRAINT DF_Units_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt   DATETIME2(7)  NULL,
    [RowVersion] ROWVERSION   NOT NULL,
    CONSTRAINT PK_Units PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Units_Code UNIQUE (Code)
);
GO

CREATE TABLE dbo.Suppliers (
    Id            INT IDENTITY(1,1) NOT NULL,
    Code          NVARCHAR(20)  NOT NULL,
    Name          NVARCHAR(150) NOT NULL,
    ContactPerson NVARCHAR(100) NULL,
    Phone         NVARCHAR(30)  NULL,
    Email         NVARCHAR(150) NULL,
    Address       NVARCHAR(300) NULL,
    IsActive      BIT           NOT NULL CONSTRAINT DF_Suppliers_IsActive  DEFAULT (1),
    CreatedAt     DATETIME2(7)  NOT NULL CONSTRAINT DF_Suppliers_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt     DATETIME2(7)  NULL,
    [RowVersion]  ROWVERSION    NOT NULL,
    CONSTRAINT PK_Suppliers PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Suppliers_Code UNIQUE (Code)
);
GO

CREATE TABLE dbo.Items (
    Id          INT IDENTITY(1,1) NOT NULL,
    Code        NVARCHAR(30)  NOT NULL,
    Name        NVARCHAR(200) NOT NULL,
    CategoryId  INT           NOT NULL,
    UnitId      INT           NOT NULL,
    MinStock    DECIMAL(18,2) NOT NULL CONSTRAINT DF_Items_MinStock DEFAULT (0),
    Description NVARCHAR(500) NULL,
    IsActive    BIT           NOT NULL CONSTRAINT DF_Items_IsActive  DEFAULT (1),
    CreatedAt   DATETIME2(7)  NOT NULL CONSTRAINT DF_Items_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt   DATETIME2(7)  NULL,
    [RowVersion] ROWVERSION   NOT NULL,
    CONSTRAINT PK_Items PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Items_Code UNIQUE (Code),
    CONSTRAINT CK_Items_MinStock CHECK (MinStock >= 0),
    CONSTRAINT FK_Items_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories (Id),
    CONSTRAINT FK_Items_Units      FOREIGN KEY (UnitId)     REFERENCES dbo.Units (Id)
);
GO
CREATE INDEX IX_Items_CategoryId ON dbo.Items (CategoryId);
CREATE INDEX IX_Items_UnitId     ON dbo.Items (UnitId);
GO

/* ============================================================================
   2. KARYAWAN
   ============================================================================ */

CREATE TABLE dbo.Employees (
    Id          INT IDENTITY(1,1) NOT NULL,
    Nik         NVARCHAR(30)  NOT NULL,
    Name        NVARCHAR(150) NOT NULL,
    Email       NVARCHAR(150) NOT NULL CONSTRAINT DF_Employees_Email    DEFAULT (N''),
    Position    NVARCHAR(100) NOT NULL CONSTRAINT DF_Employees_Position DEFAULT (N''),
    WarehouseId INT           NULL,
    UserId      NVARCHAR(450) NULL,    -- akun login (AspNetUsers.Id), diisi lewat fitur Buat akun
    IsActive    BIT           NOT NULL CONSTRAINT DF_Employees_IsActive  DEFAULT (1),
    CreatedAt   DATETIME2(7)  NOT NULL CONSTRAINT DF_Employees_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt   DATETIME2(7)  NULL,
    [RowVersion] ROWVERSION   NOT NULL,
    CONSTRAINT PK_Employees PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Employees_Nik UNIQUE (Nik),
    CONSTRAINT FK_Employees_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses (Id)
);
GO
CREATE INDEX IX_Employees_WarehouseId ON dbo.Employees (WarehouseId);
-- satu akun hanya untuk satu karyawan (boleh kosong)
CREATE UNIQUE INDEX UX_Employees_UserId ON dbo.Employees (UserId) WHERE UserId IS NOT NULL;
GO

-- FK ke tabel Identity dipasang hanya bila tabelnya sudah dibuat oleh EF Core.
IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
    ALTER TABLE dbo.Employees
        ADD CONSTRAINT FK_Employees_AspNetUsers FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id);
GO

/* ============================================================================
   3. TABEL PENDUKUNG
   ============================================================================ */

CREATE TABLE dbo.PageInfos (
    Id          INT IDENTITY(1,1) NOT NULL,
    [Key]       NVARCHAR(100) NOT NULL,   -- sama dengan alamat halaman, mis. master/barang
    Title       NVARCHAR(100) NOT NULL,
    Description NVARCHAR(300) NULL,
    Icon        NVARCHAR(30)  NULL,
    IsActive    BIT           NOT NULL CONSTRAINT DF_PageInfos_IsActive DEFAULT (1),
    CONSTRAINT PK_PageInfos PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_PageInfos_Key UNIQUE ([Key])
);
GO

CREATE TABLE dbo.DocumentCounters (
    Prefix     NVARCHAR(10) NOT NULL,   -- PO, GR, GI, SO
    [Year]     INT          NOT NULL,
    LastNumber INT          NOT NULL CONSTRAINT DF_DocumentCounters_LastNumber DEFAULT (0),
    CONSTRAINT PK_DocumentCounters PRIMARY KEY CLUSTERED (Prefix, [Year])
);
GO

/* Mengambil nomor dokumen berikutnya, mis. GR-2026-0012. Panggil dalam transaksi
   yang sama dengan pembuatan dokumen supaya tidak ada nomor ganda. */
CREATE PROCEDURE dbo.usp_NextDocumentNumber
    @Prefix NVARCHAR(10),
    @Year   INT,
    @Number NVARCHAR(30) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @n INT;

    UPDATE dbo.DocumentCounters WITH (UPDLOCK, HOLDLOCK)
       SET @n = LastNumber = LastNumber + 1
     WHERE Prefix = @Prefix AND [Year] = @Year;

    IF @@ROWCOUNT = 0
    BEGIN
        INSERT INTO dbo.DocumentCounters (Prefix, [Year], LastNumber) VALUES (@Prefix, @Year, 1);
        SET @n = 1;
    END

    SET @Number = CONCAT(@Prefix, N'-', @Year, N'-',
                         CASE WHEN @n > 9999 THEN CAST(@n AS NVARCHAR(10))
                              ELSE RIGHT(CONCAT(N'0000', @n), 4) END);
END
GO

/* ============================================================================
   4. PEMBELIAN
   ============================================================================ */

CREATE TABLE dbo.PurchaseOrders (
    Id           INT IDENTITY(1,1) NOT NULL,
    PoNumber     NVARCHAR(30)  NOT NULL,
    SupplierId   INT           NOT NULL,
    OrderDate    DATE          NOT NULL,
    ExpectedDate DATE          NULL,
    Status       NVARCHAR(20)  NOT NULL CONSTRAINT DF_PurchaseOrders_Status DEFAULT (N'Draft'),
    Notes        NVARCHAR(500) NULL,
    ApprovedBy   NVARCHAR(450) NULL,
    ApprovedAt   DATETIME2(7)  NULL,
    CreatedBy    NVARCHAR(450) NULL,
    CreatedAt    DATETIME2(7)  NOT NULL CONSTRAINT DF_PurchaseOrders_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedBy    NVARCHAR(450) NULL,
    UpdatedAt    DATETIME2(7)  NULL,
    [RowVersion] ROWVERSION    NOT NULL,
    CONSTRAINT PK_PurchaseOrders PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_PurchaseOrders_PoNumber UNIQUE (PoNumber),
    CONSTRAINT CK_PurchaseOrders_Status CHECK (Status IN (N'Draft', N'Disetujui', N'Diterima sebagian', N'Selesai', N'Dibatalkan')),
    CONSTRAINT FK_PurchaseOrders_Suppliers FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers (Id)
);
GO
CREATE INDEX IX_PurchaseOrders_SupplierId ON dbo.PurchaseOrders (SupplierId);
CREATE INDEX IX_PurchaseOrders_OrderDate  ON dbo.PurchaseOrders (OrderDate);
GO

CREATE TABLE dbo.PurchaseOrderLines (
    Id              INT IDENTITY(1,1) NOT NULL,
    PurchaseOrderId INT           NOT NULL,
    ItemId          INT           NOT NULL,
    Quantity        DECIMAL(18,2) NOT NULL,
    UnitPrice       DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseOrderLines_UnitPrice DEFAULT (0),
    CONSTRAINT PK_PurchaseOrderLines PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_PurchaseOrderLines_Item UNIQUE (PurchaseOrderId, ItemId),
    CONSTRAINT CK_PurchaseOrderLines_Quantity  CHECK (Quantity > 0),
    CONSTRAINT CK_PurchaseOrderLines_UnitPrice CHECK (UnitPrice >= 0),
    CONSTRAINT FK_PurchaseOrderLines_PurchaseOrders FOREIGN KEY (PurchaseOrderId) REFERENCES dbo.PurchaseOrders (Id) ON DELETE CASCADE,
    CONSTRAINT FK_PurchaseOrderLines_Items          FOREIGN KEY (ItemId)          REFERENCES dbo.Items (Id)
);
GO
CREATE INDEX IX_PurchaseOrderLines_ItemId ON dbo.PurchaseOrderLines (ItemId);
GO

/* ============================================================================
   5. PENERIMAAN BARANG (GR)
   ============================================================================ */

CREATE TABLE dbo.GoodsReceipts (
    Id              INT IDENTITY(1,1) NOT NULL,
    GrNumber        NVARCHAR(30)  NOT NULL,
    PurchaseOrderId INT           NULL,       -- kosong bila barang masuk tanpa PO
    SupplierId      INT           NOT NULL,
    WarehouseId     INT           NOT NULL,
    ReceiptDate     DATE          NOT NULL,
    DeliveryNoteNo  NVARCHAR(50)  NULL,
    Status          NVARCHAR(20)  NOT NULL CONSTRAINT DF_GoodsReceipts_Status DEFAULT (N'Draft'),
    Notes           NVARCHAR(500) NULL,
    PostedBy        NVARCHAR(450) NULL,
    PostedAt        DATETIME2(7)  NULL,
    CreatedBy       NVARCHAR(450) NULL,
    CreatedAt       DATETIME2(7)  NOT NULL CONSTRAINT DF_GoodsReceipts_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedBy       NVARCHAR(450) NULL,
    UpdatedAt       DATETIME2(7)  NULL,
    [RowVersion]    ROWVERSION    NOT NULL,
    CONSTRAINT PK_GoodsReceipts PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_GoodsReceipts_GrNumber UNIQUE (GrNumber),
    CONSTRAINT CK_GoodsReceipts_Status CHECK (Status IN (N'Draft', N'Diposting', N'Dibatalkan')),
    CONSTRAINT FK_GoodsReceipts_PurchaseOrders FOREIGN KEY (PurchaseOrderId) REFERENCES dbo.PurchaseOrders (Id),
    CONSTRAINT FK_GoodsReceipts_Suppliers      FOREIGN KEY (SupplierId)      REFERENCES dbo.Suppliers (Id),
    CONSTRAINT FK_GoodsReceipts_Warehouses     FOREIGN KEY (WarehouseId)     REFERENCES dbo.Warehouses (Id)
);
GO
CREATE INDEX IX_GoodsReceipts_PurchaseOrderId ON dbo.GoodsReceipts (PurchaseOrderId);
CREATE INDEX IX_GoodsReceipts_SupplierId      ON dbo.GoodsReceipts (SupplierId);
CREATE INDEX IX_GoodsReceipts_WarehouseId     ON dbo.GoodsReceipts (WarehouseId);
CREATE INDEX IX_GoodsReceipts_ReceiptDate     ON dbo.GoodsReceipts (ReceiptDate);
GO

CREATE TABLE dbo.GoodsReceiptLines (
    Id                  INT IDENTITY(1,1) NOT NULL,
    GoodsReceiptId      INT           NOT NULL,
    ItemId              INT           NOT NULL,
    PurchaseOrderLineId INT           NULL,
    Quantity            DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_GoodsReceiptLines PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_GoodsReceiptLines_Quantity CHECK (Quantity > 0),
    CONSTRAINT FK_GoodsReceiptLines_GoodsReceipts      FOREIGN KEY (GoodsReceiptId)      REFERENCES dbo.GoodsReceipts (Id) ON DELETE CASCADE,
    CONSTRAINT FK_GoodsReceiptLines_Items              FOREIGN KEY (ItemId)              REFERENCES dbo.Items (Id),
    CONSTRAINT FK_GoodsReceiptLines_PurchaseOrderLines FOREIGN KEY (PurchaseOrderLineId) REFERENCES dbo.PurchaseOrderLines (Id)
);
GO
CREATE INDEX IX_GoodsReceiptLines_GoodsReceiptId      ON dbo.GoodsReceiptLines (GoodsReceiptId);
CREATE INDEX IX_GoodsReceiptLines_ItemId              ON dbo.GoodsReceiptLines (ItemId);
CREATE INDEX IX_GoodsReceiptLines_PurchaseOrderLineId ON dbo.GoodsReceiptLines (PurchaseOrderLineId);
GO

/* ============================================================================
   6. PENGELUARAN BARANG (GI)
   ============================================================================ */

CREATE TABLE dbo.GoodsIssues (
    Id          INT IDENTITY(1,1) NOT NULL,
    GiNumber    NVARCHAR(30)  NOT NULL,
    WarehouseId INT           NOT NULL,
    IssueDate   DATE          NOT NULL,
    IssuedTo    NVARCHAR(150) NOT NULL,
    Purpose     NVARCHAR(200) NULL,
    Status      NVARCHAR(20)  NOT NULL CONSTRAINT DF_GoodsIssues_Status DEFAULT (N'Draft'),
    PostedBy    NVARCHAR(450) NULL,
    PostedAt    DATETIME2(7)  NULL,
    CreatedBy   NVARCHAR(450) NULL,
    CreatedAt   DATETIME2(7)  NOT NULL CONSTRAINT DF_GoodsIssues_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedBy   NVARCHAR(450) NULL,
    UpdatedAt   DATETIME2(7)  NULL,
    [RowVersion] ROWVERSION   NOT NULL,
    CONSTRAINT PK_GoodsIssues PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_GoodsIssues_GiNumber UNIQUE (GiNumber),
    CONSTRAINT CK_GoodsIssues_Status CHECK (Status IN (N'Draft', N'Diposting', N'Dibatalkan')),
    CONSTRAINT FK_GoodsIssues_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses (Id)
);
GO
CREATE INDEX IX_GoodsIssues_WarehouseId ON dbo.GoodsIssues (WarehouseId);
CREATE INDEX IX_GoodsIssues_IssueDate   ON dbo.GoodsIssues (IssueDate);
GO

CREATE TABLE dbo.GoodsIssueLines (
    Id           INT IDENTITY(1,1) NOT NULL,
    GoodsIssueId INT           NOT NULL,
    ItemId       INT           NOT NULL,
    Quantity     DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_GoodsIssueLines PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_GoodsIssueLines_Quantity CHECK (Quantity > 0),
    CONSTRAINT FK_GoodsIssueLines_GoodsIssues FOREIGN KEY (GoodsIssueId) REFERENCES dbo.GoodsIssues (Id) ON DELETE CASCADE,
    CONSTRAINT FK_GoodsIssueLines_Items       FOREIGN KEY (ItemId)       REFERENCES dbo.Items (Id)
);
GO
CREATE INDEX IX_GoodsIssueLines_GoodsIssueId ON dbo.GoodsIssueLines (GoodsIssueId);
CREATE INDEX IX_GoodsIssueLines_ItemId       ON dbo.GoodsIssueLines (ItemId);
GO

/* ============================================================================
   7. STOCK OPNAME
   ============================================================================ */

CREATE TABLE dbo.StockOpnames (
    Id           INT IDENTITY(1,1) NOT NULL,
    OpnameNumber NVARCHAR(30)  NOT NULL,
    WarehouseId  INT           NOT NULL,
    OpnameDate   DATE          NOT NULL,
    Status       NVARCHAR(20)  NOT NULL CONSTRAINT DF_StockOpnames_Status DEFAULT (N'Draft'),
    Notes        NVARCHAR(500) NULL,
    ApprovedBy   NVARCHAR(450) NULL,
    ApprovedAt   DATETIME2(7)  NULL,
    CreatedBy    NVARCHAR(450) NULL,
    CreatedAt    DATETIME2(7)  NOT NULL CONSTRAINT DF_StockOpnames_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedBy    NVARCHAR(450) NULL,
    UpdatedAt    DATETIME2(7)  NULL,
    [RowVersion] ROWVERSION    NOT NULL,
    CONSTRAINT PK_StockOpnames PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_StockOpnames_OpnameNumber UNIQUE (OpnameNumber),
    CONSTRAINT CK_StockOpnames_Status CHECK (Status IN (N'Draft', N'Diajukan', N'Disetujui', N'Ditolak', N'Dibatalkan')),
    CONSTRAINT FK_StockOpnames_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses (Id)
);
GO
CREATE INDEX IX_StockOpnames_WarehouseId ON dbo.StockOpnames (WarehouseId);
CREATE INDEX IX_StockOpnames_OpnameDate  ON dbo.StockOpnames (OpnameDate);
GO

CREATE TABLE dbo.StockOpnameLines (
    Id             INT IDENTITY(1,1) NOT NULL,
    StockOpnameId  INT           NOT NULL,
    ItemId         INT           NOT NULL,
    SystemQty      DECIMAL(18,2) NOT NULL,   -- stok sistem saat opname dibuat (disalin)
    CountedQty     DECIMAL(18,2) NOT NULL,   -- hasil hitung fisik
    Note           NVARCHAR(200) NULL,
    CONSTRAINT PK_StockOpnameLines PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_StockOpnameLines_Item UNIQUE (StockOpnameId, ItemId),
    CONSTRAINT CK_StockOpnameLines_CountedQty CHECK (CountedQty >= 0),
    CONSTRAINT FK_StockOpnameLines_StockOpnames FOREIGN KEY (StockOpnameId) REFERENCES dbo.StockOpnames (Id) ON DELETE CASCADE,
    CONSTRAINT FK_StockOpnameLines_Items        FOREIGN KEY (ItemId)        REFERENCES dbo.Items (Id)
);
GO
CREATE INDEX IX_StockOpnameLines_ItemId ON dbo.StockOpnameLines (ItemId);
GO

/* ============================================================================
   8. LEDGER STOK
   Hanya boleh INSERT. Positif = masuk, negatif = keluar.
   ============================================================================ */

CREATE TABLE dbo.StockMovements (
    Id                 BIGINT IDENTITY(1,1) NOT NULL,
    ItemId             INT           NOT NULL,
    WarehouseId        INT           NOT NULL,
    MovementType       NVARCHAR(20)  NOT NULL,
    Quantity           DECIMAL(18,2) NOT NULL,
    MovementDate       DATETIME2(7)  NOT NULL CONSTRAINT DF_StockMovements_MovementDate DEFAULT (SYSUTCDATETIME()),
    SourceType         NVARCHAR(30)  NOT NULL,
    SourceId           INT           NOT NULL,
    SourceNumber       NVARCHAR(30)  NOT NULL,
    ReversesMovementId BIGINT        NULL,
    CreatedBy          NVARCHAR(450) NULL,
    CreatedAt          DATETIME2(7)  NOT NULL CONSTRAINT DF_StockMovements_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_StockMovements PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_StockMovements_Quantity     CHECK (Quantity <> 0),
    CONSTRAINT CK_StockMovements_MovementType CHECK (MovementType IN (N'Receipt', N'Issue', N'Adjustment', N'Reversal')),
    CONSTRAINT CK_StockMovements_SourceType   CHECK (SourceType IN (N'GoodsReceipt', N'GoodsIssue', N'StockOpname')),
    CONSTRAINT FK_StockMovements_Items      FOREIGN KEY (ItemId)             REFERENCES dbo.Items (Id),
    CONSTRAINT FK_StockMovements_Warehouses FOREIGN KEY (WarehouseId)        REFERENCES dbo.Warehouses (Id),
    CONSTRAINT FK_StockMovements_Reverses   FOREIGN KEY (ReversesMovementId) REFERENCES dbo.StockMovements (Id)
);
GO
CREATE INDEX IX_StockMovements_Item_Warehouse_Date ON dbo.StockMovements (ItemId, WarehouseId, MovementDate) INCLUDE (Quantity);
CREATE INDEX IX_StockMovements_Source              ON dbo.StockMovements (SourceType, SourceId);
GO

-- Pengaman tambahan: baris ledger tidak boleh diubah atau dihapus.
CREATE TRIGGER dbo.TR_StockMovements_Immutable
ON dbo.StockMovements
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    THROW 50001, N'StockMovements hanya boleh ditambah, tidak boleh diubah atau dihapus. Buat mutasi pembalik.', 1;
END
GO

-- Stok saat ini per barang dan gudang.
CREATE VIEW dbo.vw_StockBalance
AS
SELECT ItemId, WarehouseId, SUM(Quantity) AS Quantity
FROM dbo.StockMovements
GROUP BY ItemId, WarehouseId;
GO

/* ============================================================================
   9. LOG
   ============================================================================ */

CREATE TABLE dbo.LoginLogs (
    Id         BIGINT IDENTITY(1,1) NOT NULL,
    UserName   NVARCHAR(256) NOT NULL,
    UserId     NVARCHAR(450) NULL,
    Success    BIT           NOT NULL,
    FailReason NVARCHAR(100) NULL,
    IpAddress  NVARCHAR(64)  NULL,
    UserAgent  NVARCHAR(512) NULL,
    CreatedAt  DATETIME2(7)  NOT NULL CONSTRAINT DF_LoginLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_LoginLogs PRIMARY KEY CLUSTERED (Id)
);
GO
CREATE INDEX IX_LoginLogs_CreatedAt ON dbo.LoginLogs (CreatedAt);
GO

CREATE TABLE dbo.ActivityLogs (
    Id         BIGINT IDENTITY(1,1) NOT NULL,
    UserId     NVARCHAR(450) NULL,
    UserName   NVARCHAR(256) NOT NULL,
    Action     NVARCHAR(20)  NOT NULL,
    EntityName NVARCHAR(100) NOT NULL,
    EntityId   NVARCHAR(50)  NOT NULL,
    Summary    NVARCHAR(300) NULL,
    OldValues  NVARCHAR(MAX) NULL,
    NewValues  NVARCHAR(MAX) NULL,
    IpAddress  NVARCHAR(64)  NULL,
    CreatedAt  DATETIME2(7)  NOT NULL CONSTRAINT DF_ActivityLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ActivityLogs PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_ActivityLogs_Action CHECK (Action IN (N'Create', N'Update', N'Delete', N'Post', N'Cancel', N'Approve'))
);
GO
CREATE INDEX IX_ActivityLogs_CreatedAt ON dbo.ActivityLogs (CreatedAt);
CREATE INDEX IX_ActivityLogs_Entity    ON dbo.ActivityLogs (EntityName, EntityId);
CREATE INDEX IX_ActivityLogs_UserId    ON dbo.ActivityLogs (UserId);
GO

CREATE TABLE dbo.ErrorLogs (
    Id            BIGINT IDENTITY(1,1) NOT NULL,
    CreatedAt     DATETIME2(7)  NOT NULL CONSTRAINT DF_ErrorLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),
    Source        NVARCHAR(20)  NOT NULL,
    [Level]       NVARCHAR(20)  NOT NULL,
    Message       NVARCHAR(1000) NOT NULL,
    ExceptionType NVARCHAR(200) NULL,
    StackTrace    NVARCHAR(MAX) NULL,
    RequestPath   NVARCHAR(300) NULL,
    HttpMethod    NVARCHAR(10)  NULL,
    StatusCode    INT           NULL,
    UserId        NVARCHAR(450) NULL,
    TraceId       NVARCHAR(64)  NULL,
    CONSTRAINT PK_ErrorLogs PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_ErrorLogs_Source CHECK (Source IN (N'Api', N'Web')),
    CONSTRAINT CK_ErrorLogs_Level  CHECK ([Level] IN (N'Warning', N'Error', N'Critical'))
);
GO
CREATE INDEX IX_ErrorLogs_CreatedAt ON dbo.ErrorLogs (CreatedAt);
CREATE INDEX IX_ErrorLogs_TraceId   ON dbo.ErrorLogs (TraceId);
GO

PRINT N'Selesai: database StockApp dan semua tabel aplikasi sudah dibuat.';
GO