IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Movies] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Director] nvarchar(100) NULL,
    [ReleaseYear] int NULL,
    [Genre] nvarchar(50) NULL,
    [Rating] decimal(3,1) NULL,
    [Description] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Movies] PRIMARY KEY ([Id])
);

CREATE TABLE [Rentals] (
    [Id] uniqueidentifier NOT NULL,
    [MovieId] uniqueidentifier NOT NULL,
    [CustomerName] nvarchar(200) NOT NULL,
    [ItemName] nvarchar(200) NOT NULL,
    [RentalDate] datetime2 NOT NULL,
    [ReturnDate] datetime2 NULL,
    [DailyRate] decimal(18,2) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Rentals] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Rentals_Movies_MovieId] FOREIGN KEY ([MovieId]) REFERENCES [Movies] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Rentals_MovieId] ON [Rentals] ([MovieId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251224112426_InitialCreate', N'10.0.1');

COMMIT;
GO

