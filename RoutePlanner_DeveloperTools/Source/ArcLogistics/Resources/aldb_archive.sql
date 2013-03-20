DELETE FROM [Stops] WHERE [RouteId] IN(
  SELECT [Id] FROM [Routes] WHERE [Default] = 0 AND [ScheduleId] IN(
    SELECT [Id] FROM [Schedules] WHERE DATEADD(D, 0, DATEDIFF(D, 0, [PlannedDate])) >= DATEADD(D, 0, DATEDIFF(D, 0, @date))))

GO
DELETE FROM [RenewalLocations] WHERE [RouteId] IN(
  SELECT [Id] FROM [Routes] WHERE [Default] = 0 AND[ScheduleId] IN(
    SELECT [Id] FROM [Schedules] WHERE DATEADD(D, 0, DATEDIFF(D, 0, [PlannedDate])) >= DATEADD(D, 0, DATEDIFF(D, 0, @date))))

GO
DELETE FROM [RouteZones] WHERE [RouteId] IN(
  SELECT [Id] FROM [Routes] WHERE [Default] = 0 AND [ScheduleId] IN(
    SELECT [Id] FROM [Schedules] WHERE DATEADD(D, 0, DATEDIFF(D, 0, [PlannedDate])) >= DATEADD(D, 0, DATEDIFF(D, 0, @date))))

GO
DELETE FROM [Routes] WHERE [Default] = 0 AND [ScheduleId] IN(
  SELECT [Id] FROM [Schedules] WHERE DATEADD(D, 0, DATEDIFF(D, 0, [PlannedDate])) >= DATEADD(D, 0, DATEDIFF(D, 0, @date)))

GO
DELETE FROM [Schedules]
WHERE DATEADD(D, 0, DATEDIFF(D, 0, [PlannedDate])) >= DATEADD(D, 0, DATEDIFF(D, 0, @date))

GO
DELETE FROM [OrderVehicleSpecialties] WHERE [OrderId] IN(
  SELECT [Id] FROM [Orders] WHERE DATEADD(D, 0, DATEDIFF(D, 0, [PlannedDate])) >= DATEADD(D, 0, DATEDIFF(D, 0, @date)))

GO
DELETE FROM [OrderDriverSpecialties] WHERE [OrderId] IN(
  SELECT [Id] FROM [Orders] WHERE DATEADD(D, 0, DATEDIFF(D, 0, [PlannedDate])) >= DATEADD(D, 0, DATEDIFF(D, 0, @date)))

GO
DELETE FROM [Orders]
WHERE DATEADD(D, 0, DATEDIFF(D, 0, [PlannedDate])) >= DATEADD(D, 0, DATEDIFF(D, 0, @date))
