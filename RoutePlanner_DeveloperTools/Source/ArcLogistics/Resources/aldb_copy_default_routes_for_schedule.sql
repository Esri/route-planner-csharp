INSERT INTO [Routes]
SELECT
    NEWID(),
    [Name],
    [WorkFrom],
    [WorkTo],
    [Breaks],
    [TimeAtStart],
    [TimeAtEnd],
    [TimeAtRenewal],
    [MaxOrders],
    [MaxTravelDistance],
    [MaxTravelDuration],
    [MaxTotalDuration],
    [Color],
    [Comment],
    0,  -- [Default]
    [Days],
    [Cost],
    [StartTime],
    [EndTime],
    [Overtime],
    [TotalTime],
    [TotalDistance],
    [TravelTime],
    [ViolationTime],
    [WaitTime],
    [Capacities],
    [Locked],
    [Visible],
    [HardZones],
    @creationTime,
    [VehiclesId],
    [DriversId],
    [StartLocationId],
    [EndLocationId],
    [Id], -- [DefaultRouteID]
    @scheduleId
FROM [Routes]
WHERE
	[Default] = 1 AND
	[Id] IN (@defaultRouteIds)
