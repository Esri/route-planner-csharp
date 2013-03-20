INSERT INTO [RenewalLocations]
SELECT [Routes].[Id], [RenewalLocations].[LocationId]
FROM [RenewalLocations]
INNER JOIN [Routes]
ON [RenewalLocations].[RouteId] = [Routes].[DefaultRouteID]
WHERE [Routes].[ScheduleId] = @scheduleId
