ALTER TABLE [Locations]
    ADD [OpenFrom2] bigint

GO
ALTER TABLE [Locations]
    ADD [OpenTo2] bigint

GO
UPDATE [PROJECT] SET [Version] = 1.2
