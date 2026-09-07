using Ai_healthcare_monitoring_and_assistance_system.Data;
using Ai_healthcare_monitoring_and_assistance_system.Models;
using Microsoft.ML;

namespace Ai_healthcare_monitoring_and_assistance_system.AI.Services;

public class AnomalyDetectionService
{
    private readonly MLContext _mlContext;
    private readonly IServiceProvider _serviceProvider;
    private const int WindowSize = 20;
    private const float SensitivityThreshold = 2.0f;

    public AnomalyDetectionService(MLContext mlContext, IServiceProvider serviceProvider)
    {
        _mlContext = mlContext;
        _serviceProvider = serviceProvider;
    }

    public List<string> DetectAnomalies(PatientReading reading)
    {
        var anomalies = new List<string>();
        if (reading.DeviceId is null) return anomalies;
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HealthMonitorDbContext>();

        AddMetric(reading, db, anomalies, "HeartRate", reading.HeartRate?.FingerDetected == true && reading.HeartRate.Bpm > 0, reading.HeartRate?.Bpm ?? 0, "ANOMALY_HEART_RATE");
        AddMetric(reading, db, anomalies, "Temperature", reading.Temperature?.SensorOk == true, reading.Temperature?.Celsius ?? 0, "ANOMALY_BODY_TEMP");
        AddMetric(reading, db, anomalies, "Acceleration", reading.Accelerometer is not null, reading.Accelerometer?.Magnitude ?? 0, "ANOMALY_ACCELERATION");
        AddMetric(reading, db, anomalies, "AmbientTemp", reading.Ambient?.SensorOk == true, reading.Ambient?.TempCelsius ?? 0, "ANOMALY_AMBIENT_TEMP");

        var cutoffDate = DateTime.UtcNow.AddDays(-30);
        var oldRecords = db.AnomalyHistory.Where(h => h.RecordedAt < cutoffDate).ToList();
        if (oldRecords.Count > 0) db.AnomalyHistory.RemoveRange(oldRecords);
        try { db.SaveChanges(); } catch { }
        return anomalies;
    }

    private void AddMetric(PatientReading reading, HealthMonitorDbContext db, List<string> anomalies, string metricType, bool available, float value, string anomaly)
    {
        if (!available) return;
        var history = db.AnomalyHistory.Where(h => h.DeviceId == reading.DeviceId && h.MetricType == metricType).OrderByDescending(h => h.RecordedAt).Take(WindowSize).Select(h => h.Value).ToList();
        var windowValues = new Queue<float>(history.AsEnumerable().Reverse());
        windowValues.Enqueue(value);
        db.AnomalyHistory.Add(new AnomalyHistoryEntity { DeviceId = reading.DeviceId!, MetricType = metricType, Value = value, RecordedAt = DateTime.UtcNow });
        if (IsOutlier(value, windowValues)) anomalies.Add(anomaly);
    }

    private bool IsOutlier(float value, Queue<float> window)
    {
        if (window.Count < 5) return false;
        var mean = window.Average();
        var stdDev = MathF.Sqrt(window.Average(x => MathF.Pow(x - mean, 2)));
        var zScore = MathF.Abs((value - mean) / (stdDev + 0.0001f));
        return zScore > SensitivityThreshold;
    }
}
