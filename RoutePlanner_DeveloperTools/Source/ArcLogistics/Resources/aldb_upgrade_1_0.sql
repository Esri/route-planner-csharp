ALTER TABLE Locations ADD
    [Unit] nvarchar(50)
GO

ALTER TABLE Orders ADD
    [Unit] nvarchar(50)
GO

UPDATE [PROJECT] SET [Version] = 1.01
