INSERT INTO [RouteZones]
SELECT [Routes].[Id], [RouteZones].[ZoneId]
FROM [RouteZones]
INNER JOIN [Routes]
ON [RouteZones].[RouteId] = [Routes].[DefaultRouteID]
WHERE [Routes].[ScheduleId] = @scheduleId
