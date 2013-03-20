ALTER TABLE [Barriers]
    ADD [BarrierType] nvarchar(256)

GO
ALTER TABLE [Barriers]
    ALTER COLUMN [StartDate] datetime NOT NULL

GO
ALTER TABLE [Barriers]
    ALTER COLUMN [FinishDate] datetime NOT NULL

GO
ALTER TABLE [Barriers]
    ALTER COLUMN [CreationTime] bigint NOT NULL

GO
ALTER TABLE [Locations]
    ALTER COLUMN [CreationTime] bigint NOT NULL

GO
ALTER TABLE [MobileDevices]
    ALTER COLUMN [CreationTime] bigint NOT NULL

GO
ALTER TABLE [Schedules]
    ALTER COLUMN [CreationTime] bigint NOT NULL

GO
ALTER TABLE [Drivers]
    ALTER COLUMN [CreationTime] bigint NOT NULL

GO
ALTER TABLE [DriverSpecialties]
    ALTER COLUMN [CreationTime] bigint NOT NULL

GO
ALTER TABLE [FuelTypes]
    ALTER COLUMN [CreationTime] bigint NOT NULL

GO
ALTER TABLE [Vehicles]
    ALTER COLUMN [CreationTime] bigint NOT NULL

GO
ALTER TABLE [VehicleSpecialties]
    ALTER COLUMN [CreationTime] bigint NOT NULL

GO
ALTER TABLE [Routes]
    ALTER COLUMN [CreationTime] bigint NOT NULL

GO
ALTER TABLE [Zones]
    ALTER COLUMN [CreationTime] bigint NOT NULL
	
GO
UPDATE [Routes]
	SET Breaks = NULL
	WHERE (Breaks LIKE '0,%')

GO
UPDATE [PROJECT] SET [Version] = 1.1
