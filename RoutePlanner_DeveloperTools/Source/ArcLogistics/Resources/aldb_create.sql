CREATE TABLE [Locations](
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50),
    [Comment] nvarchar(4000),
    [OpenFrom] bigint,
    [OpenTo] bigint,
    [OpenFrom2] bigint,
    [OpenTo2] bigint,
    [FullAddress] nvarchar(250),
    [Unit] nvarchar(50),
    [AddressLine] nvarchar(100),
    [Locality1] nvarchar(50),
    [Locality2] nvarchar(50),
    [Locality3] nvarchar(50),
    [CountyPrefecture] nvarchar(50),
    [PostalCode1] nvarchar(50),
    [PostalCode2] nvarchar(50),
    [StateProvince] nvarchar(50),
    [Country] nvarchar(50),
    [X] float,
    [Y] float,
    [Locator] nvarchar(100),
    [CurbApproach] int NOT NULL DEFAULT 0,
    [CreationTime] bigint NOT NULL,
    [Deleted] bit NOT NULL DEFAULT 0,
  CONSTRAINT [PK_Locations] PRIMARY KEY(
    [Id]
  )
)

GO
CREATE TABLE [MobileDevices](
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50),
    [ActiveSyncProfileName] nvarchar(50),
    [EmailAddress] nvarchar(50),
    [SyncFolder] nvarchar(260),
    [SyncType] int NOT NULL DEFAULT 0,
    [CreationTime] bigint NOT NULL,
    [Deleted] bit NOT NULL DEFAULT 0,
  CONSTRAINT [PK_MobileDevices] PRIMARY KEY(
    [Id]
  )
)

GO
CREATE TABLE [Schedules](
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50),
    [PlannedDate] datetime,
    [ScheduleType] int NOT NULL DEFAULT 0,
    [CreationTime] bigint NOT NULL,
  CONSTRAINT [PK_Schedules] PRIMARY KEY(
    [Id]
  )
)

GO
CREATE INDEX IDX_Schedules_PlannedDate ON [Schedules] ([PlannedDate])

GO
CREATE TABLE [Drivers](
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50),
    [Comment] nvarchar(4000),
    [FixedSalary] float NOT NULL DEFAULT 0.0,
    [PerHourSalary] float NOT NULL DEFAULT 0.0,
    [PerHourOTSalary] float NOT NULL DEFAULT 0.0,
    [TimeBeforeOT] float NOT NULL DEFAULT 0.0,
    [MobileDeviceId] uniqueidentifier,
    [CreationTime] bigint NOT NULL,
    [Deleted] bit NOT NULL DEFAULT 0,
  CONSTRAINT [PK_Drivers] PRIMARY KEY(
    [Id]
  )
)

GO
ALTER TABLE [Drivers] ADD CONSTRAINT [FK_Drivers_MobileDevices] FOREIGN KEY([MobileDeviceId])
REFERENCES [MobileDevices] ([Id])

GO
CREATE TABLE [DriverSpecialties](
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50),
    [Comment] nvarchar(4000),
    [CreationTime] bigint NOT NULL,
    [Deleted] bit NOT NULL DEFAULT 0,
  CONSTRAINT [PK_DriverSpecialties] PRIMARY KEY(
    [Id]
  )
)

GO
CREATE TABLE [DriverDriverSpecialties](
    [DriverId] uniqueidentifier NOT NULL,
    [DriverSpecialtyId] uniqueidentifier NOT NULL,
  CONSTRAINT [PK_DriverDriverSpecialties] PRIMARY KEY(
    [DriverId],
    [DriverSpecialtyId]
  )
)

GO
ALTER TABLE [DriverDriverSpecialties] ADD CONSTRAINT [FK_DriverDriverSpecialties_Drivers] FOREIGN KEY([DriverId])
REFERENCES [Drivers] ([Id])

GO
ALTER TABLE [DriverDriverSpecialties] ADD CONSTRAINT [FK_DriverDriverSpecialties_DriverSpecialties] FOREIGN KEY([DriverSpecialtyId])
REFERENCES [DriverSpecialties] ([Id])

GO
CREATE TABLE [FuelTypes](
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50),
    [Price] float NOT NULL DEFAULT 0.0,
    [Co2Emission] float NOT NULL DEFAULT 0.0,
    [CreationTime] bigint NOT NULL,
    [Deleted] bit NOT NULL DEFAULT 0,
  CONSTRAINT [PK_FuelTypes] PRIMARY KEY(
    [Id]
  )
)

GO
CREATE TABLE [Vehicles](
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50),
    [Comment] nvarchar(4000),
    [FixedSalary] float NOT NULL DEFAULT 0.0,
    [FuelConsumption] float NOT NULL DEFAULT 0.0,
    [Capacities] nvarchar(200),
    [MobileDeviceId] uniqueidentifier,
    [FuelTypeId] uniqueidentifier,
    [CreationTime] bigint NOT NULL,
    [Deleted] bit NOT NULL DEFAULT 0,
  CONSTRAINT [PK_Vehicles] PRIMARY KEY(
    [Id]
  )
)

GO
ALTER TABLE [Vehicles] ADD CONSTRAINT [FK_Vehicles_MobileDevices] FOREIGN KEY([MobileDeviceId])
REFERENCES [MobileDevices] ([Id])

GO
ALTER TABLE [Vehicles] ADD CONSTRAINT [FK_Vehicles_FuelTypes] FOREIGN KEY([FuelTypeId])
REFERENCES [FuelTypes] ([Id])

GO
CREATE TABLE [VehicleSpecialties](
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50),
    [Comment] nvarchar(4000),
    [CreationTime] bigint NOT NULL,
    [Deleted] bit NOT NULL DEFAULT 0,
  CONSTRAINT [PK_VehicleSpecialties] PRIMARY KEY(
    [Id]
  )
)

GO
CREATE TABLE [VehicleVehicleSpecialties](
    [VehicleId] uniqueidentifier NOT NULL,
    [VehicleSpecialtyId] uniqueidentifier NOT NULL,
  CONSTRAINT [PK_VehicleVehicleSpecialties] PRIMARY KEY(
    [VehicleId],
    [VehicleSpecialtyId]
  )
)

GO
ALTER TABLE [VehicleVehicleSpecialties] ADD CONSTRAINT [FK_VehicleVehicleSpecialties_Vehicles] FOREIGN KEY([VehicleId])
REFERENCES [Vehicles] ([Id])

GO
ALTER TABLE [VehicleVehicleSpecialties] ADD CONSTRAINT [FK_VehicleVehicleSpecialties_VehicleSpecialties] FOREIGN KEY([VehicleSpecialtyId])
REFERENCES [VehicleSpecialties] ([Id])

GO
CREATE TABLE [Routes](
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50),
    [WorkFrom] bigint,
    [WorkTo] bigint,
    [Breaks] nvarchar(2000),
    [TimeAtStart] float NOT NULL DEFAULT 0.0,
    [TimeAtEnd] float NOT NULL DEFAULT 0.0,
    [TimeAtRenewal] float NOT NULL DEFAULT 0.0,
    [MaxOrders] bigint NOT NULL DEFAULT 0,
    [MaxTravelDistance] float NOT NULL DEFAULT 0.0,
    [MaxTravelDuration] float NOT NULL DEFAULT 0.0,
    [MaxTotalDuration] float NOT NULL DEFAULT 0.0,
    [Color] integer NOT NULL DEFAULT 0,
    [Comment] nvarchar(4000),
    [Default] bit NOT NULL DEFAULT 0,
    [Days] nvarchar(500),
    [Cost] float NOT NULL DEFAULT 0.0,
    [StartTime] datetime,
    [EndTime] datetime,
    [Overtime] float NOT NULL DEFAULT 0.0,
    [TotalTime] float NOT NULL DEFAULT 0.0,
    [TotalDistance] float NOT NULL DEFAULT 0.0,
    [TravelTime] float NOT NULL DEFAULT 0.0,
    [ViolationTime] float NOT NULL DEFAULT 0.0,
    [WaitTime] float NOT NULL DEFAULT 0.0,
    [Capacities] nvarchar(200),
    [Locked] bit NOT NULL DEFAULT 0,
    [Visible] bit NOT NULL DEFAULT 1,
    [HardZones] bit NOT NULL DEFAULT 1,
    [CreationTime] bigint NOT NULL,
    [VehiclesId] uniqueidentifier,
    [DriversId] uniqueidentifier,
    [StartLocationId] uniqueidentifier,
    [EndLocationId] uniqueidentifier,
    [DefaultRouteID] uniqueidentifier,
    [ScheduleId] uniqueidentifier,
  CONSTRAINT [PK_Routes] PRIMARY KEY(
    [Id]
  )
)

GO
ALTER TABLE [Routes] ADD CONSTRAINT [FK_Routes_Vehicles] FOREIGN KEY([VehiclesId])
REFERENCES [Vehicles] ([Id])

GO
ALTER TABLE [Routes] ADD CONSTRAINT [FK_Routes_Drivers] FOREIGN KEY([DriversId])
REFERENCES [Drivers] ([Id])

GO
ALTER TABLE [Routes] ADD CONSTRAINT [FK_Routes_StartLocations] FOREIGN KEY([StartLocationId])
REFERENCES [Locations] ([Id])

GO
ALTER TABLE [Routes] ADD CONSTRAINT [FK_Routes_EndLocations] FOREIGN KEY([EndLocationId])
REFERENCES [Locations] ([Id])

GO
ALTER TABLE [Routes] ADD CONSTRAINT [FK_Routes_Schedules] FOREIGN KEY([ScheduleId])
REFERENCES [Schedules] ([Id])

GO
CREATE TABLE [RenewalLocations](
    [RouteId] uniqueidentifier NOT NULL,
    [LocationId] uniqueidentifier NOT NULL,
  CONSTRAINT [PK_RenewalLocations] PRIMARY KEY(
    [RouteId],
    [LocationId]
  )
)

GO
ALTER TABLE [RenewalLocations] ADD CONSTRAINT [FK_RenewalLocations_Routes] FOREIGN KEY([RouteId])
REFERENCES [Routes] ([Id])

GO
ALTER TABLE [RenewalLocations] ADD CONSTRAINT [FK_RenewalLocations_Locations] FOREIGN KEY([LocationId])
REFERENCES [Locations] ([Id])

GO
CREATE TABLE [Orders](
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50),
    [PlannedDate] datetime,
    [Locator] nvarchar(100),
    [FullAddress] nvarchar(200),
    [Unit] nvarchar(50),
    [AddressLine] nvarchar(50),
    [Locality1] nvarchar(50),
    [Locality2] nvarchar(50),
    [Locality3] nvarchar(50),
    [CountyPrefecture] nvarchar(50),
    [PostalCode1] nvarchar(50),
    [PostalCode2] nvarchar(50),
    [StateProvince] nvarchar(50),
    [Country] nvarchar(50),
    [X] float,
    [Y] float,
    [OrderType] int NOT NULL DEFAULT 0,
    [OrderPriority] int NOT NULL DEFAULT 0,
    [ServiceTime] real NOT NULL DEFAULT 0,
    [CurbApproach] int NOT NULL DEFAULT 0,
    [TW1From] bigint,
    [TW1To] bigint,
    [TW2From] bigint,
    [TW2To] bigint,
    [Capacities] nvarchar(200),
    [CustomProperties] nvarchar(4000),
    [MaxViolationTime] float,
    [CreationTime] bigint,
  CONSTRAINT [PK_Orders] PRIMARY KEY(
    [Id]
  )
)

GO
CREATE INDEX IDX_Orders_PlannedDate ON [Orders] (PlannedDate)

GO
CREATE TABLE [OrderVehicleSpecialties](
    [OrderId] uniqueidentifier NOT NULL,
    [VehicleSpecialtyId] uniqueidentifier NOT NULL,
  CONSTRAINT [PK_OrderVehicleSpecialties] PRIMARY KEY(
    [OrderId],
    [VehicleSpecialtyId]
  )
)

GO
ALTER TABLE [OrderVehicleSpecialties] ADD CONSTRAINT [FK_OrderVehicleSpecialties_Orders] FOREIGN KEY([OrderId])
REFERENCES [Orders] ([Id])

GO
ALTER TABLE [OrderVehicleSpecialties] ADD CONSTRAINT [FK_OrderVehicleSpecialties_VehicleSpecialties] FOREIGN KEY([VehicleSpecialtyId])
REFERENCES [VehicleSpecialties] ([Id])

GO
CREATE TABLE [OrderDriverSpecialties](
    [OrderId] uniqueidentifier NOT NULL,
    [DriverSpecialtyId] uniqueidentifier NOT NULL,
  CONSTRAINT [PK_OrderDriverSpecialties] PRIMARY KEY(
    [OrderId],
    [DriverSpecialtyId]
  )
)

GO
ALTER TABLE [OrderDriverSpecialties] ADD CONSTRAINT [FK_OrderDriverSpecialties_Orders] FOREIGN KEY([OrderId])
REFERENCES [Orders] ([Id])

GO
ALTER TABLE [OrderDriverSpecialties] ADD CONSTRAINT [FK_OrderDriverSpecialties_DriverSpecialties] FOREIGN KEY([DriverSpecialtyId])
REFERENCES [DriverSpecialties] ([Id])

GO
CREATE TABLE [Stops](
    [Id] uniqueidentifier NOT NULL,
    [ArriveTime] datetime,
    [Directions] image,
    [PathTo] image,
    [Distance] float NOT NULL DEFAULT 0.0,
    [SequenceNumber] int NOT NULL DEFAULT 0,
    [OrderSequenceNumber] int,
    [Type] int NOT NULL DEFAULT 0,
    [TimeAtStop] float NOT NULL DEFAULT 0.0,
    [TravelTime] float NOT NULL DEFAULT 0.0,
    [WaitTime] float NOT NULL DEFAULT 0.0,
    [Locked] bit NOT NULL DEFAULT 0,
    [LocationId] uniqueidentifier,
    [OrderId] uniqueidentifier,
    [RouteId] uniqueidentifier,
  CONSTRAINT [PK_Stops] PRIMARY KEY(
    [Id]
  )
)

GO
ALTER TABLE [Stops] ADD CONSTRAINT [FK_Stops_Locations] FOREIGN KEY([LocationId])
REFERENCES [Locations] ([Id])

GO
ALTER TABLE [Stops] ADD CONSTRAINT [FK_Stops_Orders] FOREIGN KEY([OrderId])
REFERENCES [Orders] ([Id])

GO
ALTER TABLE [Stops] ADD CONSTRAINT [FK_Stops_Routes] FOREIGN KEY([RouteId])
REFERENCES [Routes] ([Id])

GO
CREATE INDEX IDX_Stops_RouteId ON [Stops] ([RouteId])

GO
CREATE INDEX IDX_Stops_Type ON [Stops] ([Type])

GO
CREATE TABLE [Zones](
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50),
    [Comment] nvarchar(4000),
    [Geometry] image,
    [CreationTime] bigint NOT NULL,
    [Deleted] bit NOT NULL DEFAULT 0,
  CONSTRAINT [PK_Zone] PRIMARY KEY(
    [Id]
  )
)

GO
CREATE TABLE [RouteZones](
    [RouteId] uniqueidentifier NOT NULL,
    [ZoneId] uniqueidentifier NOT NULL,
  CONSTRAINT [PK_RouteZones] PRIMARY KEY(
    [RouteId],
    [ZoneId]
  )
)

GO
ALTER TABLE [RouteZones] ADD CONSTRAINT [FK_RouteZones_Routes] FOREIGN KEY([RouteId])
REFERENCES [Routes] ([Id])

GO
ALTER TABLE [RouteZones] ADD CONSTRAINT [FK_RouteZones_Zones] FOREIGN KEY([ZoneId])
REFERENCES [Zones] ([Id])

GO
CREATE TABLE [Barriers](
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50),
    [Comment] nvarchar(4000),
    [StartDate] datetime NOT NULL,
    [FinishDate] datetime NOT NULL,
    [Geometry] image,
    [BarrierType] nvarchar(256),
    [CreationTime] bigint NOT NULL,
  CONSTRAINT [PK_Barrier] PRIMARY KEY(
    [Id]
  )
)

GO
CREATE INDEX IDX_Barriers_StartDate_FinishDate ON [Barriers] ([StartDate], [FinishDate])

GO
CREATE TABLE [ConfigSchemes](
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50),
    [Value] nvarchar(4000),
  CONSTRAINT [PK_ConfigSchemes] PRIMARY KEY(
    [Id]
  )
)

GO
CREATE TABLE [Project](
    [Id] uniqueidentifier NOT NULL,
    [Version] float,
  CONSTRAINT [PK_Project] PRIMARY KEY(
    [Id]
  )
)

GO
INSERT INTO [PROJECT] ([Id], [Version]) VALUES ('03008130-206B-410d-9C40-D6A7A2E269F9', 1.2)
