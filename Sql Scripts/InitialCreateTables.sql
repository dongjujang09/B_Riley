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
GO

CREATE TABLE [Accounts] (
    [Id] int NOT NULL IDENTITY,
    [AccountName] nvarchar(100) NOT NULL,
    [Balance] float NOT NULL,
    [DateCreated] datetime2 NOT NULL,
    [DateModified] datetime2 NOT NULL,
    CONSTRAINT [PK_Accounts] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Transfers] (
    [Id] int NOT NULL IDENTITY,
    [FromAccountId] int NOT NULL,
    [ToAccountId] int NOT NULL,
    [Amount] float NOT NULL,
    [FromAccountBalance] float NOT NULL,
    [ToAccountBalance] float NOT NULL,
    [TransactionTime] datetime2 NOT NULL,
    CONSTRAINT [PK_Transfers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Transfers_Accounts_FromAccountId] FOREIGN KEY ([FromAccountId]) REFERENCES [Accounts] ([Id]),
    CONSTRAINT [FK_Transfers_Accounts_ToAccountId] FOREIGN KEY ([ToAccountId]) REFERENCES [Accounts] ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220729061621_InitialCreateTables', N'6.0.7');
GO

COMMIT;
GO

