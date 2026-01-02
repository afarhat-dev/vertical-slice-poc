-- =============================================
-- Movie Library Database Creation Script
-- =============================================
-- This script creates all tables, indexes, and foreign keys
-- for the Movie Library API with concurrent update support
-- =============================================

USE [MovieLibrary]
GO

-- =============================================
-- 1. Create Movies Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Movies]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Movies] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Director] NVARCHAR(100) NULL,
        [ReleaseYear] INT NULL,
        [Genre] NVARCHAR(50) NULL,
        [Rating] DECIMAL(3,1) NULL,
        [Description] NVARCHAR(1000) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [RowVersion] ROWVERSION NOT NULL,
        CONSTRAINT [PK_Movies] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    PRINT 'Movies table created successfully';
END
ELSE
BEGIN
    PRINT 'Movies table already exists';
END
GO

-- =============================================
-- 2. Create Rentals Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Rentals]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Rentals] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [MovieId] UNIQUEIDENTIFIER NOT NULL,
        [CustomerName] NVARCHAR(200) NOT NULL,
        [ItemName] NVARCHAR(200) NOT NULL,
        [RentalDate] DATETIME2 NOT NULL,
        [ReturnDate] DATETIME2 NULL,
        [DailyRate] DECIMAL(18,2) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [RowVersion] ROWVERSION NOT NULL,
        CONSTRAINT [PK_Rentals] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    PRINT 'Rentals table created successfully';
END
ELSE
BEGIN
    PRINT 'Rentals table already exists';
END
GO

-- =============================================
-- 3. Create RequestLogs Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RequestLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RequestLogs] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [CorrelationId] NVARCHAR(100) NOT NULL,
        [Timestamp] DATETIME2 NOT NULL,
        [HttpMethod] NVARCHAR(10) NOT NULL,
        [Path] NVARCHAR(500) NOT NULL,
        [QueryString] NVARCHAR(2000) NOT NULL,
        [RequestBody] NVARCHAR(MAX) NOT NULL,
        [SourceIpAddress] NVARCHAR(45) NOT NULL,
        [HostIpAddress] NVARCHAR(45) NOT NULL,
        [ResponseBody] NVARCHAR(MAX) NULL,
        [StatusCode] INT NULL,
        [ResponseTimeMs] BIGINT NULL,
        CONSTRAINT [PK_RequestLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    PRINT 'RequestLogs table created successfully';
END
ELSE
BEGIN
    PRINT 'RequestLogs table already exists';
END
GO

-- =============================================
-- 4. Create Foreign Keys
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Rentals_Movies_MovieId]') AND parent_object_id = OBJECT_ID(N'[dbo].[Rentals]'))
BEGIN
    ALTER TABLE [dbo].[Rentals] WITH CHECK
    ADD CONSTRAINT [FK_Rentals_Movies_MovieId]
    FOREIGN KEY([MovieId]) REFERENCES [dbo].[Movies] ([Id])
    ON DELETE NO ACTION;

    ALTER TABLE [dbo].[Rentals] CHECK CONSTRAINT [FK_Rentals_Movies_MovieId];

    PRINT 'Foreign key FK_Rentals_Movies_MovieId created successfully';
END
ELSE
BEGIN
    PRINT 'Foreign key FK_Rentals_Movies_MovieId already exists';
END
GO

-- =============================================
-- 5. Create Indexes
-- =============================================

-- Index on Rentals.MovieId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Rentals]') AND name = N'IX_Rentals_MovieId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Rentals_MovieId]
    ON [dbo].[Rentals] ([MovieId] ASC);

    PRINT 'Index IX_Rentals_MovieId created successfully';
END
ELSE
BEGIN
    PRINT 'Index IX_Rentals_MovieId already exists';
END
GO

-- Index on RequestLogs.CorrelationId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[RequestLogs]') AND name = N'IX_RequestLogs_CorrelationId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RequestLogs_CorrelationId]
    ON [dbo].[RequestLogs] ([CorrelationId] ASC);

    PRINT 'Index IX_RequestLogs_CorrelationId created successfully';
END
ELSE
BEGIN
    PRINT 'Index IX_RequestLogs_CorrelationId already exists';
END
GO

-- Index on RequestLogs.Timestamp
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[RequestLogs]') AND name = N'IX_RequestLogs_Timestamp')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RequestLogs_Timestamp]
    ON [dbo].[RequestLogs] ([Timestamp] ASC);

    PRINT 'Index IX_RequestLogs_Timestamp created successfully';
END
ELSE
BEGIN
    PRINT 'Index IX_RequestLogs_Timestamp already exists';
END
GO

-- =============================================
-- 6. Create __EFMigrationsHistory Table (EF Core requirement)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory] (
        [MigrationId] NVARCHAR(150) NOT NULL,
        [ProductVersion] NVARCHAR(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED ([MigrationId] ASC)
    );

    PRINT '__EFMigrationsHistory table created successfully';
END
ELSE
BEGIN
    PRINT '__EFMigrationsHistory table already exists';
END
GO

-- =============================================
-- 7. Insert Migration History Records
-- =============================================
IF NOT EXISTS (SELECT * FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20251224112426_InitialCreate')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251224112426_InitialCreate', N'10.0.1');

    PRINT 'InitialCreate migration recorded';
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260102120000_AddRequestLogTable')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260102120000_AddRequestLogTable', N'10.0.1');

    PRINT 'AddRequestLogTable migration recorded';
END
GO

-- =============================================
-- 8. Verify Schema
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'Database Schema Summary:';
PRINT '========================================';

SELECT
    'Tables' AS [Category],
    COUNT(*) AS [Count]
FROM sys.tables
WHERE name IN ('Movies', 'Rentals', 'RequestLogs', '__EFMigrationsHistory')

UNION ALL

SELECT
    'Foreign Keys' AS [Category],
    COUNT(*) AS [Count]
FROM sys.foreign_keys
WHERE name = 'FK_Rentals_Movies_MovieId'

UNION ALL

SELECT
    'Indexes' AS [Category],
    COUNT(*) AS [Count]
FROM sys.indexes
WHERE name IN ('IX_Rentals_MovieId', 'IX_RequestLogs_CorrelationId', 'IX_RequestLogs_Timestamp');

PRINT '';
PRINT '========================================';
PRINT 'Database creation completed successfully!';
PRINT '========================================';
GO
