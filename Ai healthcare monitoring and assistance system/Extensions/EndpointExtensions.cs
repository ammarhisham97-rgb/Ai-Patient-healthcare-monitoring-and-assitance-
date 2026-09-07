using System.Text.Json;
using Ai_healthcare_monitoring_and_assistance_system.AI.Services;
using Ai_healthcare_monitoring_and_assistance_system.Data;
using Ai_healthcare_monitoring_and_assistance_system.Models;
using Ai_healthcare_monitoring_and_assistance_system.Services;

namespace Ai_healthcare_monitoring_and_assistance_system.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapApplicationEndpoints(this WebApplication app, IConfiguration configuration)
    {
        var jwtSecret = configuration["JwtSecret"] ?? "YourSuperSecretKeyForJWTAuthenticationMinimum32Characters!!!";
        var jwtIssuer = configuration["JwtIssuer"] ?? "PatientMonitorAPI";
        var jwtAudience = configuration["JwtAudience"] ?? "PatientMonitorClient";

        app.MapPost("/api/auth/register", (RegisterRequest req, UserStore userStore, ILogger<Program> logger) =>
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password)) return Results.BadRequest(new { error = "Username and password required" });
            if (userStore.UserExists(req.Username)) return Results.BadRequest(new { error = "User already exists" });
            userStore.AddUser(req.Username, req.Password);
            logger.LogInformation("[AUTH] New user registered: {Username}", req.Username);
            return Results.Created($"/api/auth/user/{req.Username}", new { username = req.Username });
        });

        app.MapPost("/api/auth/login", (LoginRequest req, UserStore userStore, ILogger<Program> logger) =>
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password)) return Results.BadRequest(new { error = "Username and password required" });
            if (!userStore.ValidateUser(req.Username, req.Password))
            {
                logger.LogWarning("[AUTH] Failed login attempt: {Username}", req.Username);
                return Results.Unauthorized();
            }
            var token = JwtTokenGenerator.Generate(req.Username, jwtSecret, jwtIssuer, jwtAudience);
            logger.LogInformation("[AUTH] User logged in: {Username}", req.Username);
            return Results.Ok(new { token, expiresIn = 3600, tokenType = "Bearer" });
        });

        app.MapPost("/api/readings", (PatientReading reading, ReadingsStore store, AnomalyDetectionService anomalyService, ILogger<Program> logger) =>
        {
            if (reading is null || string.IsNullOrWhiteSpace(reading.DeviceId)) return Results.BadRequest(new { error = "Invalid payload" });
            reading.ReceivedAt = DateTime.UtcNow;
            var anomalies = anomalyService.DetectAnomalies(reading);
            if (ReadingAnalysisService.DetectFall(reading, logger)) anomalies.Add("ANOMALY_FALL_DETECTED");
            reading.ActivityLevel = ReadingAnalysisService.DetectActivity(reading);
            reading.DetectedAnomalies = new List<string>(anomalies);
            var entity = new PatientReadingEntity
            {
                DeviceId = reading.DeviceId, Timestamp = reading.Timestamp, ReceivedAt = reading.ReceivedAt,
                ActivityLevel = reading.ActivityLevel, DetectedAnomalies = JsonSerializer.Serialize(reading.DetectedAnomalies), ReadingData = JsonSerializer.Serialize(reading)
            };
            try { store.Add(entity); } catch (Exception ex) { logger.LogError(ex, "Error saving reading to database"); }
            ReadingLogger.Log(reading, logger);
            var alerts = ReadingAnalysisService.BuildAlerts(reading);
            alerts.AddRange(anomalies);
            if (alerts.Count > 0) logger.LogWarning("[ALERT] Active alerts: {Alerts}", string.Join(", ", alerts));
            return Results.Ok(new { status = "received", alerts, anomalies, activityLevel = reading.ActivityLevel, stored = store.Count, serverTime = reading.ReceivedAt });
        });

        app.MapGet("/api/readings", (ReadingsStore store) => Results.Ok(store.GetAll()));
        app.MapGet("/api/readings/latest", (ReadingsStore store) =>
        {
            var latest = store.GetLatest();
            return latest is null ? Results.NotFound(new { error = "No readings yet" }) : Results.Ok(latest);
        });
        app.MapGet("/api/readings/{deviceId}", (string deviceId, ReadingsStore store) => Results.Ok(store.GetByDevice(deviceId)));
        app.MapGet("/api/anomalies/summary", (ReadingsStore store) =>
        {
            var readings = store.GetAll().ToList();
            return Results.Ok(new
            {
                totalReadings = readings.Count,
                readingsWithAnomalies = readings.Count(r => !string.IsNullOrEmpty(r.DetectedAnomalies)),
                commonAnomalies = readings.Where(r => !string.IsNullOrEmpty(r.DetectedAnomalies)).SelectMany(r => JsonSerializer.Deserialize<List<string>>(r.DetectedAnomalies!) ?? new()).GroupBy(a => a).OrderByDescending(g => g.Count()).Select(g => new { anomaly = g.Key, count = g.Count() }).ToList()
            });
        });
        app.MapGet("/api/activity/summary", (ReadingsStore store) =>
        {
            var readings = store.GetAll().ToList();
            return Results.Ok(new
            {
                totalReadings = readings.Count,
                activityBreakdown = new { sedentary = readings.Count(r => r.ActivityLevel == "Sedentary"), light = readings.Count(r => r.ActivityLevel == "Light"), moderate = readings.Count(r => r.ActivityLevel == "Moderate"), vigorous = readings.Count(r => r.ActivityLevel == "Vigorous") },
                recentActivity = readings.Take(10).Select(r => new { time = r.ReceivedAt, activity = r.ActivityLevel })
            });
        });
        app.MapGet("/api/status", (ReadingsStore store) => Results.Ok(new { status = "online", totalStored = store.Count, serverTime = DateTime.UtcNow }));

        MapReportEndpoints(app);
        return app;
    }

    private static void MapReportEndpoints(WebApplication app)
    {
        app.MapGet("/api/reports/daily/{deviceId}", async (string deviceId, ReportService service, ILogger<Program> logger) => { var date = DateTime.UtcNow.Date; var report = await service.GenerateDailyReportAsync(deviceId, date); logger.LogInformation("[REPORT] Daily report generated for device {DeviceId}", deviceId); return Results.Ok(report); });
        app.MapGet("/api/reports/daily/{deviceId}/pdf", async (string deviceId, ReportService service, ILogger<Program> logger) => { var date = DateTime.UtcNow.Date; var report = await service.GenerateDailyReportAsync(deviceId, date); var bytes = PdfReportGenerator.GenerateDailyReportPdf(report); logger.LogInformation("[REPORT] Daily PDF generated for device {DeviceId}", deviceId); return Results.File(bytes, "application/pdf", $"Daily_Report_{deviceId}_{date:yyyy-MM-dd}.pdf"); });
        app.MapGet("/api/reports/weekly/{deviceId}", async (string deviceId, ReportService service, ILogger<Program> logger) => { var today = DateTime.UtcNow.Date; var start = today.AddDays(-(int)today.DayOfWeek); var report = await service.GenerateWeeklyReportAsync(deviceId, start); logger.LogInformation("[REPORT] Weekly report generated for device {DeviceId}", deviceId); return Results.Ok(report); });
        app.MapGet("/api/reports/weekly/{deviceId}/pdf", async (string deviceId, ReportService service, ILogger<Program> logger) => { var today = DateTime.UtcNow.Date; var start = today.AddDays(-(int)today.DayOfWeek); var report = await service.GenerateWeeklyReportAsync(deviceId, start); var bytes = PdfReportGenerator.GenerateWeeklyReportPdf(report); logger.LogInformation("[REPORT] Weekly PDF generated for device {DeviceId}", deviceId); return Results.File(bytes, "application/pdf", $"Weekly_Report_{deviceId}_{start:yyyy-MM-dd}.pdf"); });
        app.MapGet("/api/reports/monthly/{deviceId}", async (string deviceId, ReportService service, ILogger<Program> logger) => { var today = DateTime.UtcNow.Date; var report = await service.GenerateMonthlyReportAsync(deviceId, today.Year, today.Month); logger.LogInformation("[REPORT] Monthly report generated for device {DeviceId}", deviceId); return Results.Ok(report); });
        app.MapGet("/api/reports/monthly/{deviceId}/pdf", async (string deviceId, ReportService service, ILogger<Program> logger) => { var today = DateTime.UtcNow.Date; var report = await service.GenerateMonthlyReportAsync(deviceId, today.Year, today.Month); var bytes = PdfReportGenerator.GenerateMonthlyReportPdf(report); logger.LogInformation("[REPORT] Monthly PDF generated for device {DeviceId}", deviceId); return Results.File(bytes, "application/pdf", $"Monthly_Report_{deviceId}_{today.Year}-{today.Month:D2}.pdf"); });
        app.MapGet("/api/reports/alerts/{deviceId}", async (ReportService service, ILogger<Program> logger, string deviceId, int daysBack = 30) => { var to = DateTime.UtcNow; var report = await service.GenerateAlertHistoryReportAsync(deviceId, to.AddDays(-daysBack), to); logger.LogInformation("[REPORT] Alert history report generated for device {DeviceId}", deviceId); return Results.Ok(report); });
        app.MapGet("/api/reports/alerts/{deviceId}/pdf", async (ReportService service, ILogger<Program> logger, string deviceId, int daysBack = 30) => { var to = DateTime.UtcNow; var report = await service.GenerateAlertHistoryReportAsync(deviceId, to.AddDays(-daysBack), to); var bytes = PdfReportGenerator.GenerateAlertHistoryReportPdf(report); logger.LogInformation("[REPORT] Alert history PDF generated for device {DeviceId}", deviceId); return Results.File(bytes, "application/pdf", $"Alert_History_{deviceId}_{DateTime.UtcNow:yyyy-MM-dd}.pdf"); });
        app.MapGet("/api/reports/anomalies/{deviceId}", async (ReportService service, ILogger<Program> logger, string deviceId, int daysBack = 30) => { var to = DateTime.UtcNow; var report = await service.GenerateAnomalyReportAsync(deviceId, to.AddDays(-daysBack), to); logger.LogInformation("[REPORT] Anomaly report generated for device {DeviceId}", deviceId); return Results.Ok(report); });
        app.MapGet("/api/reports/anomalies/{deviceId}/pdf", async (ReportService service, ILogger<Program> logger, string deviceId, int daysBack = 30) => { var to = DateTime.UtcNow; var report = await service.GenerateAnomalyReportAsync(deviceId, to.AddDays(-daysBack), to); var bytes = PdfReportGenerator.GenerateAnomalyReportPdf(report); logger.LogInformation("[REPORT] Anomaly PDF generated for device {DeviceId}", deviceId); return Results.File(bytes, "application/pdf", $"Anomaly_Report_{deviceId}_{DateTime.UtcNow:yyyy-MM-dd}.pdf"); });
    }
}
