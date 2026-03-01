-- ============================================================
-- TurnosDB - Complete Database Setup Script
-- Generated for SQL Server Express
-- ============================================================

USE master;
GO

-- Create database if not exists
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TurnosDB')
BEGIN
    CREATE DATABASE TurnosDB;
    PRINT 'Database TurnosDB created successfully.';
END
ELSE
    PRINT 'Database TurnosDB already exists.';
GO

USE TurnosDB;
GO

-- ============================================================
-- DROP TABLES (in correct order to avoid FK conflicts)
-- ============================================================
IF OBJECT_ID('dbo.Appointments', 'U') IS NOT NULL DROP TABLE dbo.Appointments;
IF OBJECT_ID('dbo.Branches', 'U') IS NOT NULL DROP TABLE dbo.Branches;
IF OBJECT_ID('dbo.AdminUsers', 'U') IS NOT NULL DROP TABLE dbo.AdminUsers;
IF OBJECT_ID('dbo.__EFMigrationsHistory', 'U') IS NOT NULL DROP TABLE dbo.__EFMigrationsHistory;
GO

-- ============================================================
-- CREATE TABLES
-- ============================================================

-- AdminUsers
CREATE TABLE dbo.AdminUsers (
    Id          UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    Username    NVARCHAR(50)        NOT NULL,
    PasswordHash NVARCHAR(MAX)      NOT NULL,
    FullName    NVARCHAR(100)       NOT NULL,
    Role        NVARCHAR(50)        NOT NULL DEFAULT 'Admin',
    IsActive    BIT                 NOT NULL DEFAULT 1,
    CreatedAt   DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_AdminUsers PRIMARY KEY (Id)
);
GO

-- Branches
CREATE TABLE dbo.Branches (
    Id          UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    Name        NVARCHAR(100)       NOT NULL,
    Address     NVARCHAR(200)       NOT NULL,
    City        NVARCHAR(100)       NOT NULL,
    IsActive    BIT                 NOT NULL DEFAULT 1,
    CONSTRAINT PK_Branches PRIMARY KEY (Id)
);
GO

-- Appointments
CREATE TABLE dbo.Appointments (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    Code                NVARCHAR(20)        NOT NULL,
    CustomerIdNumber    NVARCHAR(20)        NOT NULL,
    BranchId            UNIQUEIDENTIFIER    NOT NULL,
    Status              NVARCHAR(50)        NOT NULL DEFAULT 'Pending',
    CreatedAt           DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt           DATETIME2           NOT NULL,
    ActivatedAt         DATETIME2           NULL,
    AttendedAt          DATETIME2           NULL,
    CONSTRAINT PK_Appointments PRIMARY KEY (Id),
    CONSTRAINT FK_Appointments_Branches FOREIGN KEY (BranchId)
        REFERENCES dbo.Branches(Id) ON DELETE RESTRICT
);
GO

-- ============================================================
-- INDEXES
-- ============================================================
CREATE UNIQUE INDEX IX_AdminUsers_Username
    ON dbo.AdminUsers(Username);

CREATE INDEX IX_Appointments_CustomerIdNumber
    ON dbo.Appointments(CustomerIdNumber);

CREATE INDEX IX_Appointments_CustomerIdNumber_CreatedAt
    ON dbo.Appointments(CustomerIdNumber, CreatedAt);

CREATE INDEX IX_Appointments_Status
    ON dbo.Appointments(Status);

CREATE INDEX IX_Appointments_BranchId
    ON dbo.Appointments(BranchId);
GO

-- ============================================================
-- EF MIGRATIONS HISTORY
-- ============================================================
CREATE TABLE dbo.__EFMigrationsHistory (
    MigrationId     NVARCHAR(150)   NOT NULL,
    ProductVersion  NVARCHAR(32)    NOT NULL,
    CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
);

INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20240101000000_InitialCreate', '8.0.0');
GO

-- ============================================================
-- SEED DATA - Admin Users
-- Password: AdminPass5955.*
-- ============================================================
INSERT INTO dbo.AdminUsers (Id, Username, PasswordHash, FullName, Role, IsActive, CreatedAt)
VALUES (
    NEWID(),
    'admin',
    '$2a$12$w2vPAygNnI6bpb2FIvqcmOPDSst1TQiarok3I1YxgSiizl.M5YXR6',
    'Administrador Principal',
    'Admin',
    1,
    GETUTCDATE()
);
GO

-- ============================================================
-- SEED DATA - Branches (Medellín)
-- ============================================================
INSERT INTO dbo.Branches (Id, Name, Address, City, IsActive) VALUES
(NEWID(), 'Sucursal El Poblado',      'Carrera 43A # 7-50',       'Medellín', 1),
(NEWID(), 'Sucursal Laureles',        'Circular 73 # 39-50',      'Medellín', 1),
(NEWID(), 'Sucursal Envigado',        'Calle 37 Sur # 43B-20',    'Medellín', 1),
(NEWID(), 'Sucursal Bello',           'Carrera 49 # 51-30',       'Medellín', 1),
(NEWID(), 'Sucursal Itagüí',          'Calle 50 # 52-20',         'Medellín', 1),
(NEWID(), 'Sucursal Centro Medellín', 'Carrera 52 # 42-30',       'Medellín', 1),
(NEWID(), 'Sucursal Sabaneta',        'Calle 77 Sur # 43-20',     'Medellín', 1),
(NEWID(), 'Sucursal Castilla',        'Carrera 65 # 98-40',       'Medellín', 1),
(NEWID(), 'Sucursal Robledo',         'Carrera 80 # 65-30',       'Medellín', 1),
(NEWID(), 'Sucursal Belén',           'Carrera 76 # 33-50',       'Medellín', 1);
GO

-- ============================================================
-- SEED DATA - Test Appointments
-- ============================================================
DECLARE @BranchId1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.Branches WHERE Name = 'Sucursal El Poblado');
DECLARE @BranchId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.Branches WHERE Name = 'Sucursal Laureles');
DECLARE @BranchId3 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.Branches WHERE Name = 'Sucursal Envigado');

INSERT INTO dbo.Appointments (Id, Code, CustomerIdNumber, BranchId, Status, CreatedAt, ExpiresAt, ActivatedAt, AttendedAt)
VALUES
-- Pending appointments
(NEWID(), 'T080001001', '5555555555', @BranchId1, 'Pending',   GETUTCDATE(), DATEADD(MINUTE, 15, GETUTCDATE()), NULL, NULL),
(NEWID(), 'T080002002', '4444444444', @BranchId2, 'Pending',   GETUTCDATE(), DATEADD(MINUTE, 15, GETUTCDATE()), NULL, NULL),
-- Active appointments
(NEWID(), 'T080003003', '3333333333', @BranchId3, 'Active',    GETUTCDATE(), DATEADD(MINUTE, 15, GETUTCDATE()), GETUTCDATE(), NULL),
(NEWID(), 'T080004004', '2222222222', @BranchId1, 'Active',    GETUTCDATE(), DATEADD(MINUTE, 15, GETUTCDATE()), GETUTCDATE(), NULL),
-- Attended appointments
(NEWID(), 'T080005005', '1111111111', @BranchId2, 'Attended',  DATEADD(HOUR, -2, GETUTCDATE()), DATEADD(MINUTE, -105, GETUTCDATE()), DATEADD(HOUR, -2, GETUTCDATE()), DATEADD(MINUTE, -110, GETUTCDATE())),
(NEWID(), 'T080006006', '1111111111', @BranchId3, 'Attended',  DATEADD(HOUR, -3, GETUTCDATE()), DATEADD(MINUTE, -165, GETUTCDATE()), DATEADD(HOUR, -3, GETUTCDATE()), DATEADD(MINUTE, -170, GETUTCDATE())),
-- Expired appointments
(NEWID(), 'T080007007', '6666666666', @BranchId1, 'Expired',   DATEADD(HOUR, -1, GETUTCDATE()), DATEADD(MINUTE, -45, GETUTCDATE()), NULL, NULL),
(NEWID(), 'T080008008', '7777777777', @BranchId2, 'Expired',   DATEADD(HOUR, -2, GETUTCDATE()), DATEADD(MINUTE, -105, GETUTCDATE()), NULL, NULL),
-- Cancelled appointments
(NEWID(), 'T080009009', '8888888888', @BranchId3, 'Cancelled', DATEADD(HOUR, -1, GETUTCDATE()), DATEADD(MINUTE, -45, GETUTCDATE()), NULL, NULL),
(NEWID(), 'T080010010', '9999999999', @BranchId1, 'Cancelled', DATEADD(HOUR, -2, GETUTCDATE()), DATEADD(MINUTE, -105, GETUTCDATE()), NULL, NULL);
GO

-- ============================================================
-- STATISTICS QUERIES
-- ============================================================

-- Total appointments by status today
SELECT
    Status,
    COUNT(*) AS Total
FROM dbo.Appointments
WHERE CAST(CreatedAt AS DATE) = CAST(GETUTCDATE() AS DATE)
GROUP BY Status
ORDER BY Total DESC;

-- Total appointments by branch today
SELECT
    b.Name AS Branch,
    COUNT(a.Id) AS Total,
    SUM(CASE WHEN a.Status = 'Attended' THEN 1 ELSE 0 END) AS Attended,
    SUM(CASE WHEN a.Status = 'Pending'  THEN 1 ELSE 0 END) AS Pending,
    SUM(CASE WHEN a.Status = 'Active'   THEN 1 ELSE 0 END) AS Active,
    SUM(CASE WHEN a.Status = 'Expired'  THEN 1 ELSE 0 END) AS Expired,
    SUM(CASE WHEN a.Status = 'Cancelled'THEN 1 ELSE 0 END) AS Cancelled
FROM dbo.Branches b
LEFT JOIN dbo.Appointments a ON b.Id = a.BranchId
    AND CAST(a.CreatedAt AS DATE) = CAST(GETUTCDATE() AS DATE)
GROUP BY b.Name
ORDER BY Total DESC;

-- Customers with most appointments today
SELECT
    CustomerIdNumber,
    COUNT(*) AS Total
FROM dbo.Appointments
WHERE CAST(CreatedAt AS DATE) = CAST(GETUTCDATE() AS DATE)
GROUP BY CustomerIdNumber
ORDER BY Total DESC;

-- Average wait time (minutes) for attended appointments today
SELECT
    ROUND(AVG(DATEDIFF(MINUTE, CreatedAt, AttendedAt)), 2) AS AvgWaitTimeMinutes
FROM dbo.Appointments
WHERE Status = 'Attended'
AND CAST(CreatedAt AS DATE) = CAST(GETUTCDATE() AS DATE);
GO

PRINT 'TurnosDB setup completed successfully.';
GO